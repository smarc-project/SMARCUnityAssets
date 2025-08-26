using UnityEngine;
using Unity.Robotics.Core;
using ROS.Core;
using RosMessageTypes.Std;
using DefaultNamespace.Water;
using DefaultNamespace;


namespace ROS.Publishers
{
    class SmarcAltitude : ROSBehaviour
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
                Debug.LogError("base_link not found for smarc altitude");
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
            float altitude = base_link.position.y;
            // if we are underwater, we need a raycast down to get altitude from the ground
            // if there is no hit, that means we are underwater, but there is no ground...
            // so infinite altitude?
            if (depth > 0)
            {
                if (Physics.Raycast(base_link.position, Vector3.down, out RaycastHit hit))
                {
                    altitude = hit.distance;
                }
                else
                {
                    altitude = float.PositiveInfinity;
                }
            }
            msg.data = altitude;
            rosCon.Publish(topic, msg);
        }
    }
}
