using UnityEngine;
using Unity.Robotics.Core;
using VehicleComponents.ROS.Core;
using RosMessageTypes.Std;
using VehicleComponents.Sensors;
using System.Collections.Generic;
using DefaultNamespace;


namespace VehicleComponents.ROS.Publishers
{
    class SmarcBatteryPercent : ROSBehaviour
    {
        [Header("ROS Publisher")]
        public float frequency = 10f;
        float period => 1.0f/frequency;
        double lastUpdate = 0f;
        bool registered = false;

        Transform base_link;
        List<Battery> batteries;
        Float32Msg msg;


        protected override void StartROS()
        {
            msg = new Float32Msg();

            var robot = Utils.FindParentWithTag(gameObject, "robot", false);
            base_link = Utils.FindDeepChildWithName(robot, "base_link").transform;
            if (base_link == null)
            {
                Debug.LogError("base_link not found for smarc battery.");
                enabled = false;
                return;
            }

            if (!registered)
            {
                rosCon.RegisterPublisher<Float32Msg>(topic);
                registered = true;
            }

            // Find all Battery components in the children of this GameObject
            batteries = new List<Battery>(base_link.GetComponentsInChildren<Battery>());
        }

        void FixedUpdate()
        {
            if (Clock.Now - lastUpdate < period) return;
            lastUpdate = Clock.Now;
            // Calculate the average battery percentage
            float totalBatteryPercentage = 0f;
            foreach (var battery in batteries)
            {
                totalBatteryPercentage += battery.currentPercent;
            }
            float averageBatteryPercentage = totalBatteryPercentage / batteries.Count;
            msg.data = averageBatteryPercentage;
            rosCon.Publish(topic, msg);
        }
    }
}
