using UnityEngine;
using RosMessageTypes.Smarc;
using Unity.Robotics.Core; // Clock

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
        }

        protected override void UpdateMessage()
        {
            if(prop == null) return;

            ROSMsg.rpm.rpm = (int)prop.rpm;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}