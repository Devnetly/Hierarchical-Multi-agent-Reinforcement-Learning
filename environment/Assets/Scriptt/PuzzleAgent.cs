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

    public float moveSpeed = 10f;

    private Vector3 initialPosition;

    public bool FoundCheckpoint = false;

    public bool thisAgentLeft = false;

    protected override void Awake()
    {
        base.Awake();
        // MaxStep = 0;
        initialPosition = transform.localPosition;
    }

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();

        //get the parent object of the agent
        Transform parent = transform.parent;
        pressurePlates = parent.GetComponentsInChildren<Transform>()
                        .Where(child => child.CompareTag("plate"))
                        .Select(child => child.gameObject)
                        .OrderBy(plate => plate.name)
                        .ToArray();
        if (pressurePlates.Length < 2)
        {
            Debug.LogError("Not enough pressure plates found!");
            return;
        }
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
        if (col.gameObject.GetComponent<PuzzleAgent>() == this)
        {
            thisAgentLeft = true;
        }
    }

    public void EnteredFirstStage(Collider col, float reward)
    {
        if (col.gameObject.GetComponent<PuzzleAgent>() == this)
        {
            thisAgentLeft = false;
        }
    }
}
