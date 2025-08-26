using UnityEngine;
using RosMessageTypes.Std;
using Unity.Robotics.Core; // Clock
using Utils = DefaultNamespace.Utils;

using Propeller = VehicleComponents.Actuators.Propeller;
using ROS.Core;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(Propeller))]
    public class PropellerFeedback_Pub: ROSPublisher<Float32Msg, Propeller>
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
        }

        protected override void UpdateMessage()
        {
            if(prop == null) return;

            ROSMsg.data = (int)prop.rpm;
        }
    }
}
