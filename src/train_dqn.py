import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim
from mlagents_envs.environment import UnityEnvironment, ActionTuple
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

# Set device
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f"Using device: {device}")

# Actor-Critic Network
class ActorCritic(nn.Module):
    def __init__(self, state_dim, action_dim):
        super(ActorCritic, self).__init__()
        self.actor = nn.Sequential(
            nn.Linear(state_dim, 128),
            nn.ReLU(),
            nn.Linear(128, 128),
            nn.ReLU(),
            nn.Linear(128, action_dim),
            nn.Tanh(),
        ).to(device)
        
        self.critic = nn.Sequential(
            nn.Linear(state_dim, 128),
            nn.ReLU(),
            nn.Linear(128, 128),
            nn.ReLU(),
            nn.Linear(128, 1),
        ).to(device)

    def forward(self, state):
        action_mean = self.actor(state)
        value = self.critic(state)
        return action_mean, value

# PPO Trainer
class PPO:
    def __init__(self, state_dim, action_dim, lr, gamma, eps_clip, k_epochs):
        self.policy = ActorCritic(state_dim, action_dim).to(device)
        self.optimizer = optim.Adam(self.policy.parameters(), lr=lr)
        self.gamma = gamma
        self.eps_clip = eps_clip
        self.k_epochs = k_epochs
        self.mse_loss = nn.MSELoss()

    def compute_advantage(self, rewards, values, dones):
        returns = []
        G = 0
        for r, v, d in zip(reversed(rewards), reversed(values), reversed(dones)):
            G = r + self.gamma * G * (1 - d)
            returns.insert(0, G)
        returns = torch.tensor(returns, dtype=torch.float32, device=device)
        advantage = returns - torch.tensor(values, dtype=torch.float32, device=device)
        return returns, advantage

    def update(self, states, actions, rewards, dones, old_log_probs, values):
        returns, advantages = self.compute_advantage(rewards, values, dones)

        for _ in range(self.k_epochs):
            # Convert to tensors on GPU
            states_tensor = torch.tensor(states, dtype=torch.float32, device=device)
            actions_tensor = torch.tensor(actions, dtype=torch.float32, device=device)
            old_log_probs_tensor = torch.tensor(old_log_probs, dtype=torch.float32, device=device)
            advantages_tensor = torch.tensor(advantages, dtype=torch.float32, device=device)
            returns_tensor = torch.tensor(returns, dtype=torch.float32, device=device)

            # Evaluate policy
            action_means, values = self.policy(states_tensor)
            dist = torch.distributions.Normal(action_means, torch.tensor(0.1, device=device))
            log_probs = dist.log_prob(actions_tensor).sum(axis=-1)
            entropy = dist.entropy().mean()

            # Calculate ratios
            ratios = torch.exp(log_probs - old_log_probs_tensor)

            # PPO Loss
            surr1 = ratios * advantages_tensor
            surr2 = torch.clamp(ratios, 1 - self.eps_clip, 1 + self.eps_clip) * advantages_tensor
            actor_loss = -torch.min(surr1, surr2).mean()
            critic_loss = self.mse_loss(values.squeeze(), returns_tensor)
            loss = actor_loss + 0.5 * critic_loss - 0.01 * entropy

            # Backpropagation
            self.optimizer.zero_grad()
            loss.backward()
            self.optimizer.step()

# Training PPO
def train_ppo():
    # Unity Environment
    channel = EngineConfigurationChannel()
    env = UnityEnvironment(file_name="../build/roller.x86_64", side_channels=[channel])
    channel.set_configuration_parameters(time_scale=20.0)
    env.reset()

    behavior_name = list(env.behavior_specs.keys())[0]
    spec = env.behavior_specs[behavior_name]

    # Hyperparameters
    state_dim = 8
    action_dim = 2
    lr = 0.0003
    gamma = 0.99
    eps_clip = 0.2
    k_epochs = 4
    total_episodes = 1000
    max_steps = 1000

    ppo = PPO(state_dim, action_dim, lr, gamma, eps_clip, k_epochs)

    for episode in range(total_episodes):
        env.reset()
        decision_steps, terminal_steps = env.get_steps(behavior_name)
        state = decision_steps.obs[0][0]
        states, actions, rewards, dones, log_probs, values = [], [], [], [], [], []
        episode_reward = 0

        for _ in range(max_steps):
            # Sample action
            state_tensor = torch.tensor(state, dtype=torch.float32, device=device)
            action_mean, value = ppo.policy(state_tensor)
            dist = torch.distributions.Normal(action_mean, torch.tensor(0.1, device=device))
            action = dist.sample()
            log_prob = dist.log_prob(action).sum()

            # Move action to CPU for numpy conversion
            action_np = action.cpu().detach().numpy()
            
            # Interact with environment
            action_tuple = ActionTuple(continuous=np.array([action_np]))
            env.set_actions(behavior_name, action_tuple)
            env.step()

            # Get new state and reward
            decision_steps, terminal_steps = env.get_steps(behavior_name)
            if len(decision_steps) > 0:
                next_state = decision_steps.obs[0][0]
                reward = decision_steps.reward[0]
                done = False
            else:
                next_state = terminal_steps.obs[0][0]
                reward = terminal_steps.reward[0]
                done = True

            # Store episode data
            states.append(state)
            actions.append(action_np)
            rewards.append(reward)
            dones.append(done)
            log_probs.append(log_prob.cpu().item())
            values.append(value.cpu().item())

            state = next_state
            episode_reward += reward

            if done:
                break

        # Update PPO
        ppo.update(states, actions, rewards, dones, log_probs, values)
        print(f"Episode {episode} ended with reward: {episode_reward}")
        if episode % 100 == 0:  # e.g., save_interval = 100
            torch.save(ppo.policy, f"ppo_model_episode_{episode}.pth")

    env.close()

if __name__ == "__main__":
    train_ppo()
