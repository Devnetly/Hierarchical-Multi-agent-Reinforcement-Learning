using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class EnvController : MonoBehaviour
{
    [System.Serializable]
    public class AgentInfo
    {
        public PuzzleAgent agent;

        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;

        [HideInInspector]
        public float distanceToPlate0;
        [HideInInspector]
        public float distanceToPlate1;
    }

    public List<AgentInfo> agents = new List<AgentInfo>();
    private int resetTimer;
    public int MaxEnvironmentSteps = 50000;
    public SimpleMultiAgentGroup agentGroup;

    private GameObject block, checkPoint;
    private Vector3 blockStartingPos;
    private Quaternion blockStartingRot;

    [Range(0, 50)]
    public float checkPointOffset = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agentGroup = new SimpleMultiAgentGroup();
        foreach (AgentInfo agent in agents)
        {
            agent.StartingPos = agent.agent.transform.position;
            agent.StartingRot = agent.agent.transform.rotation;
            agent.Rb = agent.agent.GetComponent<Rigidbody>();
            agentGroup.RegisterAgent(agent.agent);
        }

        //get the block child object
        block = transform.Find("Block").gameObject;
        if (block != null)
        {
            blockStartingPos = block.transform.position;
            blockStartingRot = block.transform.rotation;
        }
        else
        {
            Debug.LogError("Block not found in the environment hierarchy.");
        }

        //get the checkpoint child object
        checkPoint = transform.Find("checkpoint").gameObject;
        if (checkPoint != null)
        {
            checkPoint.transform.position += Vector3.forward * Academy.Instance.EnvironmentParameters.GetWithDefault("checkPointOffset", checkPointOffset); 
        }
        else
        {
            Debug.LogError("CheckPoint not found in the environment hierarchy.");
        }
    }

    void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            agentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].distanceToPlate0 = Vector3.Distance(agents[i].agent.transform.position, agents[i].agent.pressurePlates[0].transform.position);
            agents[i].distanceToPlate1 = Vector3.Distance(agents[i].agent.transform.position, agents[i].agent.pressurePlates[1].transform.position);

            //if agent on either plate add a reward
            if (agents[i].distanceToPlate0 < 2.25f || agents[i].distanceToPlate1 < 2.25f)
            {
                agents[i].agent.AddReward(0.25f / MaxEnvironmentSteps);
            }

            //if agent left the room add a reward
            if (agents[i].agent.thisAgentLeft)
            {
                agents[i].agent.AddReward(0.5f / MaxEnvironmentSteps);
            }


            //if the other agent is still in the first room while the current agent is on the plate
            if (!agents[1 - i].agent.thisAgentLeft && (agents[i].distanceToPlate0 < 2.25f || agents[i].distanceToPlate1 < 2.25f))
            {
                agentGroup.AddGroupReward(-2 / MaxEnvironmentSteps);
                agents[1-i].agent.AddReward(-0.5f / MaxEnvironmentSteps);
                Debug.Log("Other agent still in the room while this agent is on the plate");
            }
            else if (agents[1 - i].agent.thisAgentLeft && !agents[i].agent.thisAgentLeft) //if other agent left and this one is still in the room
            {
                agentGroup.AddGroupReward(-4 / MaxEnvironmentSteps);
                agents[i].agent.AddReward(-1 / MaxEnvironmentSteps);
                Debug.Log("Other agent left the room and this one is still in the room");
            }
        }

        if(agents[0].agent.thisAgentLeft && agents[1].agent.thisAgentLeft)
        {
            agentGroup.AddGroupReward(0.5f / MaxEnvironmentSteps);
            Debug.Log("Both agents left the room");
        }

        //Hurry Up Penalty
        agentGroup.AddGroupReward(-0.25f / MaxEnvironmentSteps);
    }

    void Update()
    {
        
    }

    private void ResetScene()
    {
        resetTimer = 0;
        foreach (AgentInfo agent in agents)
        {
            agent.agent.transform.position = agent.StartingPos;
            agent.agent.transform.rotation = agent.StartingRot;
            agent.Rb.linearVelocity = Vector3.zero;
            agent.Rb.angularVelocity = Vector3.zero;
            agent.agent.thisAgentLeft = false;
            agent.agent.FoundCheckpoint = false;
        }

        //reset block position
        if (block != null)
        {
            block.transform.position = blockStartingPos;
            block.transform.rotation = blockStartingRot;
            Rigidbody blockRb = block.GetComponent<Rigidbody>();
            if (blockRb != null)
            {
                blockRb.linearVelocity = Vector3.zero;
                blockRb.angularVelocity = Vector3.zero;
            }
        }
    }

    public void FoundFlag(Collider col, float reward) 
    {
        Debug.Log("Flag Found");
        col.gameObject.SetActive(false);
        agentGroup.AddGroupReward(reward);
        agentGroup.EndGroupEpisode();
        ResetScene();
    }

    public void FoundCheckpoint(Collider cpCol, float reward)
    {
        // if all agents found the checkpoint then give reward
        bool allFound = true;
        foreach (AgentInfo agent in agents)
        {
            if (!agent.agent.FoundCheckpoint)
            {
                allFound = false;
                break;
            }
        }
        if (allFound)
        {
            Debug.Log("All agents found checkpoint");
            agentGroup.AddGroupReward(reward);
            agentGroup.EndGroupEpisode();
            ResetScene();
        }
    }
}
