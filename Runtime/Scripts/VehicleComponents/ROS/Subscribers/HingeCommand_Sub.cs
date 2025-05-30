using UnityEngine;

using RosMessageTypes.Sam; // ThrusterAngles,
using Hinge = VehicleComponents.Actuators.Hinge;

namespace VehicleComponents.ROS.Subscribers
{

    [RequireComponent(typeof(Hinge))]
    public class HingeCommand_Sub : Actuator_Sub<ThrusterAnglesMsg>
    {        
        public enum AngleChoice
        {
            vertical,
            horizontal
        }
        [Header("Thrust vector command")]
        [Tooltip("ThrusterAngles contains both vertical and horizontal angles. Pick one that applies to this hinge.")]
        public AngleChoice angleChoice = AngleChoice.vertical;
        Hinge hinge;

        void Awake()
        {
            hinge = GetComponent<Hinge>();
        }


        protected override void UpdateVehicle(bool reset)
        {
            if(hinge == null)
            {
                Debug.Log($"Hingecommand Sub found no hinge to command! Disabling.");
                enabled = false;
                rosCon.Unsubscribe(topic);
                return;
            }
            
            if(reset)
            {
                hinge.SetAngle(0);
                return;
            }
            var angle=0f;
            if(angleChoice == AngleChoice.vertical) angle = ROSMsg.thruster_vertical_radians;
            else angle = ROSMsg.thruster_horizontal_radians;
            hinge.SetAngle(angle);
        }
    }
}

