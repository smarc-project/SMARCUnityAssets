using UnityEngine;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;
using RosMessageTypes.PsdkInterfaces;
using DefaultNamespace;

using ROS.Core;
using Force;
using Unity.Robotics.Core;

namespace M350.PSDK_ROS2
{
    public abstract class PsdkBase<RosMsgType> : ROSBehaviour
        where RosMsgType: ROSMessage, new()
    {
        protected string tf_prefix;

        [Header("PsdkBase")]
        public float frequency = 10f;
        float period => 1.0f / frequency;
        double lastUpdate = 0f;
        bool registered = false;

        protected RosMsgType ROSMsg;
        protected MixedBody body;

        void Awake()
        {
            var robot = Utils.FindParentWithTag(gameObject, "robot", false);
            if (robot == null)
            {
                Debug.LogError("Robot not found!");
                enabled = false;
                return;
            }
            tf_prefix = robot.name;

            var base_link = Utils.FindDeepChildWithName(robot, "base_link").transform;
            if (base_link == null)
            {
                Debug.LogError("base_link not found!");
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

        }

        protected override void StartROS()
        {
            ROSMsg = new RosMsgType();
            if (!registered)
            {
                rosCon.RegisterPublisher<RosMsgType>(topic);
                registered = true;
            }
            InitPublisher();
        }

        protected abstract void UpdateMessage();
        protected virtual void InitPublisher() { }

        void FixedUpdate()
        {
            if (Clock.Now - lastUpdate < period) return;
            lastUpdate = Clock.Now;
            UpdateMessage();
            rosCon.Publish(topic, ROSMsg);
        }


    }
}