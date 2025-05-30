using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // Correct namespace for std_srvs/Trigger

public class ULBDropper : MonoBehaviour
{
    public GameObject ULB; // Reference to the ULB prefab

    private ROSConnection ros;
    public string serviceName = "/BROV2/drop_service";  // ROS service name
    public int pingers_left = 3;

    void Start()
    {
        // Initialize ROS connection
        ros = ROSConnection.GetOrCreateInstance();

        // Register the ROS service
        ros.ImplementService<TriggerRequest, TriggerResponse>(serviceName, BROV2_drop_service);
        // Create and register the service handler
        
        Debug.Log($"Service server registered: {serviceName}");
    }

    /// <summary>
    /// Callback to handle the drop service request.
    /// The method name must match the service name, replacing '/' with '_'.
    /// </summary>
    public TriggerResponse BROV2_drop_service(TriggerRequest request)
    {
        TriggerResponse response = new TriggerResponse();

        // Try to drop the ULB
        bool success = DropULB();

        if (success)
        {
            response.success = true;
            response.message = "ULB dropped successfully!";
            Debug.Log(response.message);
        }
        else
        {
            response.success = false;
            response.message = "Failed to drop ULB.";
            Debug.LogError(response.message);
        }

        return response;
    }

    /// <summary>
    /// Drops the ULB at the current position and orientation.
    /// </summary>
    private bool DropULB()
    {
        if (pingers_left == 0)
        {
            return false;
        }

        // Instantiate the ULB at the current position and orientation
        Instantiate(ULB, transform.position, transform.rotation * ULB.transform.rotation);
        pingers_left--;
        return true;
    }
}
