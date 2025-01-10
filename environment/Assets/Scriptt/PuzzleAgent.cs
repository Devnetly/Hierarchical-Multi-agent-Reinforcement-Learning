using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class PuzzleAgent : Agent
{
    private Rigidbody rBody;
    private Animator doorAnim; // Reference to the door's animator

    public float moveSpeed = 10f;

    private Vector3 initialPosition;

    public bool FoundCheckpoint = false;

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
    }
      

    public override void OnEpisodeBegin()
    {
        // Reset agent and environment
        this.rBody.linearVelocity = Vector3.zero;
        this.transform.localPosition = initialPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {      
        sensor.AddObservation(doorAnim.GetBool("Opening"));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        // Add reward if door is opening
        if (doorAnim.GetBool("Opening"))
        {
            AddReward(0.5f);
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
}
