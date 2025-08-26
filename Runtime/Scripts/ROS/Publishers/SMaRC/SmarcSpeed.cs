using UnityEngine;
using Unity.Robotics.Core;
using ROS.Core;
using RosMessageTypes.Std;
using Force;
using DefaultNamespace;


namespace ROS.Publishers
{
    class SmarcSpeed : ROSBehaviour
    {
        [Header("ROS Publisher")]
        public float frequency = 10f;
        float period => 1.0f/frequency;
        double lastUpdate = 0f;
        bool registered = false;

        Float32Msg msg;

        MixedBody body;


        protected override void StartROS()
        {
            msg = new Float32Msg();

            var robot = Utils.FindParentWithTag(gameObject, "robot", false);
            var base_link = Utils.FindDeepChildWithName(robot, "base_link").transform;
            if( base_link == null)
            {
                Debug.LogError("base_link not found for smarc speed.");
                enabled = false;
                return;
            }

            var base_link_ab = base_link.GetComponent<ArticulationBody>();
            var base_link_rb = base_link.GetComponent<Rigidbody>();
            body = new MixedBody(base_link_ab, base_link_rb);
            if (!body.isValid)
            {
                Debug.LogError("Base link doesnt have a valid Rigidbody or ArticulationBody.");
                enabled = false;
                return;
            }
            
            if (!registered)
            {
                rosCon.RegisterPublisher<Float32Msg>(topic);
                registered = true;
            }
        }

        void FixedUpdate()
        {
            if (Clock.Now - lastUpdate < period) return;
            lastUpdate = Clock.Now;
            var speed = body.velocity.magnitude;
            msg.data = speed;
            // Publish the message
            rosCon.Publish(topic, msg);
        }
    }
}
