using UnityEngine;
using RosMessageTypes.Smarc;
using Unity.Robotics.Core; // Clock
using Utils = DefaultNamespace.Utils;

using Propeller = VehicleComponents.Actuators.Propeller;
using VehicleComponents.ROS.Core;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(Propeller))]
    public class PropellerFeedback_Pub: ROSPublisher<ThrusterFeedbackMsg, Propeller>
    {
        Propeller prop;
        protected override void InitPublisher()
        {
            prop = GetComponent<Propeller>();
            if(prop == null)
            {
                Debug.Log("No propeller found!");
                enabled = false;
                return;
            }
            var robotGO = Utils.FindParentWithTag(gameObject, "robot", false);
            string prefix = robotGO.name;
            ROSMsg.header.frame_id = $"{prefix}/{sensor.linkName}";
        }

        protected override void UpdateMessage()
        {
            if(prop == null) return;

            ROSMsg.rpm.rpm = (int)prop.rpm;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}
