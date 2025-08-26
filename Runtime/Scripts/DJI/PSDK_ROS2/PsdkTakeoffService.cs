using UnityEngine;
using RosMessageTypes.Std;
using dji;

using ROS.Core;


namespace M350.PSDK_ROS2
{
    public class PsdkTakeoffService : ROSBehaviour
    {
        bool registered = false;
        public float takeoffAlt = 5; //Actual drone takes off to 2 meters, but thsi ensures that one can have it move around succesfully and gives some error on height
        public float takeoffError = .1f;
        DJIController controller = null;


        protected override void StartROS()
        {
            if(controller == null){
                controller = GetComponentInParent<DJIController>();
            }
            if (!registered)
            {
                rosCon.ImplementService<TriggerRequest, TriggerResponse>(topic, _takeoff_callback);
                registered = true;
            }
        }

        private TriggerResponse _takeoff_callback(TriggerRequest request){
            TriggerResponse response = new TriggerResponse();
            Debug.Log("Take off service running");
            if(controller == null){
                controller = GetComponentInParent<DJIController>();
                Debug.Log("Finding Controller Component");
            }

            if(controller != null){
                Debug.Log("Controller not Null");
                if(controller.controllerType == ControllerType.FLU_Velocity && controller.position.y < takeoffAlt - takeoffError){
                    controller.isTakingOff = true;
                    controller.isLanding = false;
                    controller.isLanded = false;
                    Debug.Log("Setting takingOff to true");
                    controller.target_alt = takeoffAlt;
                    response.success = true;
                }
                else{ 
                    Debug.Log("Either controller Type or alt is wrong");
                    response.success = false;
                    return response;
                }
                return response;
            }
            else{
                Debug.Log("Controller Null! Can't Take off");
                response.success = false;
                return response;
            }
        }

    }
}