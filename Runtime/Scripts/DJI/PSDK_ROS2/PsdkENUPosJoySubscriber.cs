using UnityEngine;
using RosMessageTypes.Sensor;

using ROS.Core;
using Unity.Robotics.Core;
using dji;


namespace M350.PSDK_ROS2
{
    public class PsdkENUPosJoySubscriber : ROSBehaviour
    {
        public float joy_timeout = 1;
        public float time_since_joy;

        bool registered = false;
        DJIController controller = null;


        protected override void StartROS(){
            if(controller == null){
                controller = GetComponentInParent<DJIController>();
            }

            JoyMsg ROSMsg = new JoyMsg();
            if (!registered)
            {
                rosCon.Subscribe<JoyMsg>(topic, _ENU_pos_joy_sub_callback);
                registered = true;
            }
        }

        void _ENU_pos_joy_sub_callback(JoyMsg msg){
            if(controller == null){
                controller = GetComponentInParent<DJIController>();
            }
            if(controller != null){
                time_since_joy = (float)Clock.time - msg.header.stamp.sec - msg.header.stamp.nanosec / Mathf.Pow(10f,9f);
                controller.controllerType = (ControllerType)1; //Position Control
                if(time_since_joy  < joy_timeout){
                    controller.commandPositionENU.x = msg.axes[0];
                    controller.commandPositionENU.y = msg.axes[1];
                    controller.commandPositionENU.z = msg.axes[2];
                }
                else{
                    controller.commandPositionENU.x = 0;
                    controller.commandPositionENU.y = 0;
                    controller.commandVelocityFLU.z = 0;
                }
            }
            
        }

    }
}