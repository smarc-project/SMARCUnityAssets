using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;  // for PoseStampedMsg

/*
 * RosWaypointFollower.cs
 * 
 * This script subscribes to a ROS 2 topic publishing PoseStamped waypoints
 * and teleports the Unity GameObject to each new waypoint position.
 * 
 * - Converts coordinates from ROS (ENU) to Unity (FLU) convention.
 * - Uses ROS TCP Connector to subscribe to /setpoint_position topic.
 * - Updates GameObject's position instantly upon receiving each message.
 */


public class RosWaypointFollower : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("ROS topic publishing PoseStamped waypoints at 1 Hz")]
    public string topicName = "/setpoint_position";

    Vector3 m_RosPosition;
    Vector3 m_UnityPosition;
    bool m_HasTarget = false;

    void Start()
    {
        // Grab (or create) the ROSConnection singleton
        var ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PoseStampedMsg>(topicName, PoseStampedCallback);
    }

    void PoseStampedCallback(PoseStampedMsg msg)
    {
        m_RosPosition = new Vector3(
            (float)msg.pose.position.x,
            (float)msg.pose.position.y,
            (float)msg.pose.position.z
        );
        m_UnityPosition = RosToUnityPosition(m_RosPosition);
        m_HasTarget = true;


    }

    static Vector3 RosToUnityPosition(Vector3 ros)
    {
        return new Vector3(ros.x, ros.z, ros.y);
    }

    void Update()
    {
        if (!m_HasTarget)
            return;

        // **INSTANTLY** move the drone to the new waypoint:
        transform.position = m_UnityPosition;

        // mark as done so we only teleport once per message
        m_HasTarget = false;
        Debug.Log($"Teleported to {m_UnityPosition}");
    }
}
