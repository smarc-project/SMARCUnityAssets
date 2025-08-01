using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using VehicleComponents.Sensors;
using RosMessageTypes.PsdkInterfaces;
using dji;



namespace M350.PSDK_ROS2
{
    public class PsdkControlMode : PsdkBase<ControlModeMsg>
    {
        private DJIController controller = null;

        protected override void InitPublisher(){
            controller = GetComponentInParent<DJIController>(); //Get current control state from the controller itself
        }

        protected override void UpdateMessage()
        {
            if(controller == null){
                controller = GetComponentInParent<DJIController>();
            }
            if(controller != null){
                if(controller.controllerType == (dji.ControllerType)2){ //Checks whether it is in attitude control mode.
                    ROSMsg.control_auth = 0;
                }
                else{
                    ROSMsg.control_auth = 1;
                }
                ROSMsg.device_mode = 4;
            }
        }
    }
}