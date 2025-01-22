import numpy as np
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.envs.unity_parallel_env import UnityParallelEnv
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from mlagents_envs.envs.unity_pettingzoo_base_env import UnityPettingzooBaseEnv

channel = EngineConfigurationChannel()
env = UnityEnvironment(file_name="../build/puzzle.x86_64")
#channel.set_configuration_parameters(time_scale=20.0)
#env = UnityParallelEnv(unity_env)
env.reset()