using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using DefaultNamespace;


namespace VehicleComponents.ROS.Core
{
    /// <summary>
    /// Base class for all ROS behaviours. This class handles the connection to ROS and the topic name.
    /// It also provides a method to initialize the ros-related objects.
    /// </summary>
    public abstract class ROSBehaviour : MonoBehaviour
    {
        [Header("ROS Behaviour")]
        protected ROSConnection rosCon;
        public string topic = "";


        void OnEnable()
        {
            // we gotta check this stuff all the time
            // beacuse we can enable and disable this component at runtime.
            // and we need to make sure we have a connection to ROS and a topic name.
            rosCon = ROSConnection.GetOrCreateInstance();
            if(rosCon == null)
            {
                Debug.Log($"ROSCon null for {gameObject.name} -> {topic}. Disabling.");
                enabled = false;
                return;
            }

            if (topic == null || topic == "")
            {
                Debug.Log($"ROS topic is not set for {gameObject.name}! Disabling.");
                enabled = false;
                return;
            }
            
            // Aldready in root namespace, dont touch.
            if(topic[0] != '/')
            {
                // We namespace the topic with the robot name
                GameObject robotGO;
                if(gameObject.CompareTag("robot"))
                {
                    robotGO = gameObject;
                }
                else
                {
                    robotGO = Utils.FindParentWithTag(gameObject, "robot", false);
                }

                if(robotGO == null)
                {
                    Debug.LogError($"No #robot tagged self/parent found for {gameObject.name} with topic {topic} (which is not global), disabling.");
                    enabled = false;
                    return;
                }

                string robot_name = robotGO.name;
                if(robot_name == null)
                {
                    Debug.LogWarning($"ROS topic is not namespaced with a robot name for {gameObject.name}! It will be under `/`");
                    topic = $"/{topic}";
                }
                else topic = $"/{robot_name}/{topic}";
            }

            StartROS();
        }



        void Start()
        {
            // ROS stuff should be off by default, but we still want to init them if they were enabled on game start
            // and THEN disable them
            enabled = false;
        }



        /// <summary>
        /// Override this method to initialize the ROS-related objects.
        /// This method is called after the ROS connection is established and the topic name is set.
        /// If you don't need any initialization, you can ignore it.
        /// </summary>
        protected virtual void StartROS(){}

    }
}