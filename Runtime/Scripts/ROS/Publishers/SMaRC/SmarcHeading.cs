using UnityEngine;
using Unity.Robotics.Core;
using ROS.Core;
using RosMessageTypes.Std;
using DefaultNamespace;


namespace VehicleComponents.ROS.Publishers
{
    class SmarcHeading : ROSBehaviour
    {
        [Header("ROS Publisher")]
        public float frequency = 10f;
        float period => 1.0f/frequency;
        double lastUpdate = 0f;
        bool registered = false;

        Transform base_link;
        Float32Msg msg;


        protected override void StartROS()
        {
            msg = new Float32Msg();
            // find base link under the robot-tagged top-level object
            var robot = Utils.FindParentWithTag(gameObject, "robot", false);
            base_link = Utils.FindDeepChildWithName(robot, "base_link").transform;
            if (base_link == null)
            {
                Debug.LogError("base_link not found for smarcheading.");
                enabled = false;
                return;
            }
            if(!registered)
            {
                rosCon.RegisterPublisher<Float32Msg>(topic);
                registered = true;
            }
        }

        void FixedUpdate()
        {
            if (Clock.Now - lastUpdate < period) return;
            lastUpdate = Clock.Now;
            var angle = Vector3.SignedAngle(Vector3.forward, base_link.forward, Vector3.up);
            msg.data = angle;
            // Publish the message
            rosCon.Publish(topic, msg);
        }
    }
}
