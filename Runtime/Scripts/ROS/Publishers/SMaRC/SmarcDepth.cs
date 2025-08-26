using UnityEngine;
using Unity.Robotics.Core;
using ROS.Core;
using RosMessageTypes.Std;
using DefaultNamespace.Water;
using DefaultNamespace;


namespace ROS.Publishers
{
    class SmarcDepth : ROSBehaviour
    {
        [Header("ROS Publisher")]
        public float frequency = 10f;
        float period => 1.0f/frequency;
        double lastUpdate = 0f;
        bool registered = false;

        Transform base_link;
        Float32Msg msg;

        WaterQueryModel waterQueryModel;

        protected override void StartROS()
        {
            var robot = Utils.FindParentWithTag(gameObject, "robot", false);
            base_link = Utils.FindDeepChildWithName(robot, "base_link").transform;
            msg = new Float32Msg();
            if (base_link == null)
            {
                Debug.LogError("base_link not found for smarc depth.");
                enabled = false;
                return;
            }
            if (!registered)
            {
                rosCon.RegisterPublisher<Float32Msg>(topic);
                registered = true;
            }
            waterQueryModel = FindFirstObjectByType<WaterQueryModel>();
            if (waterQueryModel == null)
            {
                Debug.LogError("WaterQueryModel not found in the scene.");
                enabled = false;
                return;
            }

        }

        void FixedUpdate()
        {
            if (Clock.Now - lastUpdate < period) return;
            lastUpdate = Clock.Now;
            var waterSurfaceLevel = waterQueryModel.GetWaterLevelAt(base_link.position);
            float depth = waterSurfaceLevel - base_link.position.y;
            msg.data = depth;
            rosCon.Publish(topic, msg);
        }
    }
}
