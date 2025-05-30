using UnityEngine;

using RosMessageTypes.Smarc; // ThrusterRPM
using Propeller = VehicleComponents.Actuators.Propeller;

namespace VehicleComponents.ROS.Subscribers
{
    [RequireComponent(typeof(Propeller))]
    public class PropellerCommand_Sub : Actuator_Sub<ThrusterRPMMsg>
    {        
        Propeller prop;
        
        void Awake()
        {
            prop = GetComponent<Propeller>();
        }

        protected override void UpdateVehicle(bool reset)
        {
            if(prop == null)
            {
                Debug.Log($"[{transform.name}] No propeller found! Disabling.");
                enabled = false;
                rosCon.Unsubscribe(topic);
                return;
            }

            if(reset)
            { 
                prop.SetRpm(0);
                return;
            }

            prop.SetRpm(ROSMsg.rpm);
        }
    }
}