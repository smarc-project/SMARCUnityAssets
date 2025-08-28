using RosMessageTypes.Std;
using dji;

using ROS.Core;
using VehicleComponents.Sensors;

namespace M350.PSDK_ROS2
{
    public class PsdkLandingService : ROSBehaviour
    {
        bool registered = false;
        public float landingAlt = 3; 
        public float landingError = .05f;
        DJIController controller = null;
        LockedDirectionDepthSensor depthSensor = null;


        protected override void StartROS()
        {
            if(controller == null){
                controller = GetComponentInParent<DJIController>();
            }
            if(controller != null && depthSensor == null){
                depthSensor = controller.GetComponentInChildren<LockedDirectionDepthSensor>();
            }
            if (!registered)
            {
                rosCon.ImplementService<TriggerRequest, TriggerResponse>(topic, _landing_callback);
                registered = true;
            }
        }

        private TriggerResponse _landing_callback(TriggerRequest request){
            TriggerResponse response = new TriggerResponse();
            if(controller == null){
                controller = GetComponentInParent<DJIController>();
            }
            if(controller != null && depthSensor == null){
                depthSensor = controller.GetComponentInChildren<LockedDirectionDepthSensor>();
            }

            if(controller != null && depthSensor != null){
                if(controller.controllerType == (dji.ControllerType)0){
                    controller.isTakingOff = false;
                    controller.isLanding = true;
                    controller.target_alt = controller.position.y - depthSensor.depth;
                    response.success = true;
                }
                else{ 
                    response.success = false;
                    return response;
                }
                return response;
            }
            else{
                response.success = false;
                return response;
            }
        }
    }
}