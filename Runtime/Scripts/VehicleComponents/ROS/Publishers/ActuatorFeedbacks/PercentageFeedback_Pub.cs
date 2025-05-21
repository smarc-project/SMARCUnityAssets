using UnityEngine;
using RosMessageTypes.Smarc;
using Unity.Robotics.Core; // Clock

using IPercentageActuator = VehicleComponents.Actuators.IPercentageActuator;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(IPercentageActuator))]
    public class PercentageFeedback_Pub: ROSPublisher<PercentStampedMsg, IPercentageActuator>
    {
        IPercentageActuator act;
        protected override void InitPublisher()
        {
            act = GetComponent<IPercentageActuator>();
            if(act == null)
            {
                Debug.Log("No IPercentageActuator found!");
                enabled = false;
                return;
            }
        }

        protected override void UpdateMessage()
        {
            if(act == null) return;
            ROSMsg.value = (float)act.GetCurrentValue();
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}
