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
    }

    public List<AgentInfo> agents = new List<AgentInfo>();

    public Camera cam;
    private int resetTimer;
    public int MaxEnvironmentSteps = 50000;
    public SimpleMultiAgentGroup agentGroup;


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
    }

    void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            agentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        //Hurry Up Penalty
        agentGroup.AddGroupReward(-0.5f / MaxEnvironmentSteps);
    }

    void Update()
    {
        //camera follows the first agent only on z axis
        Vector3 camPos = cam.transform.position;
        camPos.z = agents[0].agent.transform.position.z - 10;
        cam.transform.position = camPos;
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
            cpCol.gameObject.SetActive(false);
            agentGroup.AddGroupReward(reward);
        }
    }
}
