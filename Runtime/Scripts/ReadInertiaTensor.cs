using UnityEngine;

public class ReadInertiaTensor : MonoBehaviour
{
    void Start()
    {
        // Get the ArticulationBody component attached to this GameObject
        ArticulationBody articulationBody = GetComponent<ArticulationBody>();

        if (articulationBody != null)
        {
            // Read the inertia tensor
            Vector3 inertiaTensor = articulationBody.inertiaTensor;

            // Log the inertia tensor to the console
            Debug.Log($"Inertia Tensor: {inertiaTensor}");
        }
        else
        {
            Debug.LogError("No ArticulationBody component found on this GameObject.");
        }
    }
}