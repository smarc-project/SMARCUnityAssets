using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using RosMessageTypes.Smarc; // PercentStamped
using IPercentageActuator = VehicleComponents.Actuators.IPercentageActuator;

namespace VehicleComponents.ROS.Subscribers
{

    [RequireComponent(typeof(IPercentageActuator))]
    public class PercentageCommand_Sub : Actuator_Sub<PercentStampedMsg>
    {
        IPercentageActuator act;

        void Awake()
        {
            act = GetComponent<IPercentageActuator>();
            if(act == null)
            {
                Debug.Log("No IPercentageActuator found!");
                return;
            }
        }

        protected override void UpdateVehicle(bool reset)
        {
            if(act == null) return;
            if(reset) act.SetPercentage(act.GetResetValue());
            else act.SetPercentage(ROSMsg.value);
        }
    }
}
