using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class PuzzleAgent : Agent
{
    public GameObject[] pressurePlates;

    private Rigidbody rBody;
    private Animator doorAnim; // Reference to the door's animator

    public float moveSpeed = 10f;

    private Vector3 initialPosition;

    public bool FoundCheckpoint = false;

    public bool otherAgentLeft = false, thisAgentLeft = false;

    public EnvController envController;

    protected override void Awake()
    {
        base.Awake();
       // MaxStep = 0;
        initialPosition = transform.localPosition;
    }

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        doorAnim = GameObject.FindGameObjectWithTag("door").GetComponent<Animator>();
        pressurePlates = GameObject.FindGameObjectsWithTag("plate");
        pressurePlates = pressurePlates.OrderBy(plate => plate.name).ToArray();
        envController = GetComponentInParent<EnvController>();
    }
      

    public override void OnEpisodeBegin()
    {
        // Reset agent and environment
        this.rBody.linearVelocity = Vector3.zero;
        this.transform.localPosition = initialPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {      
        sensor.AddObservation(pressurePlates[0].GetComponent<OpenDoor>().isPressed);
        sensor.AddObservation(pressurePlates[1].GetComponent<OpenDoor>().isPressed);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        float distanceToPlate0 = Vector3.Distance(transform.position, pressurePlates[0].transform.position);
        float distanceToPlate1 = Vector3.Distance(transform.position, pressurePlates[1].transform.position);

        //if agent on either plate add a reward
        if (distanceToPlate0 < 2.25f || distanceToPlate1 < 2.25f)
        {
            AddReward(1f/envController.MaxEnvironmentSteps);
            Debug.Log("adding reward");
        }

        //if the other agent is still in the same room while the current agent is on the plate
        if(!otherAgentLeft && distanceToPlate0 < 2.25f)
        {
            envController.agentGroup.AddGroupReward(-8f/envController.MaxEnvironmentSteps);
            Debug.Log("adding reward1");
        }
        else if(!thisAgentLeft && otherAgentLeft) //if the other agent has left the room while the current agent hasnt
        {
            AddReward(-4f/envController.MaxEnvironmentSteps);
            Debug.Log("adding reward2");
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        rBody.AddForce(dirToGo * moveSpeed,
            ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public void LeftFirstStage(Collider col, float reward)
    {
        if(col.gameObject.GetComponent<PuzzleAgent>() != this)
        {
            otherAgentLeft = true;
        }
        else if(col.gameObject.GetComponent<PuzzleAgent>() == this)
        {
            thisAgentLeft = true;
        }
    }
}
