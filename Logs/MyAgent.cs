using UnityEngine;
using MLAgents;

public class MyAgent : Agent
{
    private Rigidbody rb;
    public Transform target;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Collect all the necessary observations from the agent
    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect agent's position [x, y, z]
        sensor.AddObservation(transform.localPosition);
        
        // Collect agent's velocity [vx, vy, vz]
        sensor.AddObservation(rb.velocity);
        
        // Collect target position [tx, ty, tz]
        sensor.AddObservation(target.localPosition);
    }

    // Define what actions to take when the model gives output (e.g., move the agent)
    public override void OnActionReceived(float[] vectorAction)
    {
        Vector3 action = new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]);
        rb.velocity = action;  // Apply action to the agent's velocity
    }

    // Optional: For manual control, use the Heuristic method
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        actionsOut[1] = 0;
        actionsOut[2] = 0;
    }
}
