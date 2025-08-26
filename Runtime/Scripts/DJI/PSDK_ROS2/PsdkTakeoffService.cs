using UnityEngine;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;
using RosMessageTypes.Std;
using DefaultNamespace;
using dji;

using ROS.Core;
using Force;
using Unity.Robotics.Core;

namespace M350.PSDK_ROS2
{
    public class PsdkTakeoffService : ROSBehaviour
    {
        protected string tf_prefix;

        bool registered = false;
        public float takeoffAlt = 5; //Actual drone takes off to 2 meters, but thsi ensures that one can have it move around succesfully and gives some error on height
        public float takeoffError = .1f;
        DJIController controller = null;



        protected MixedBody body;

        void Awake()
        {
            var robot = Utils.FindParentWithTag(gameObject, "robot", false);
            if (robot == null)
            {
                Debug.LogError("Robot not found!");
                enabled = false;
                return;
            }
            tf_prefix = robot.name;

            var base_link = Utils.FindDeepChildWithName(robot, "base_link").transform;
            if (base_link == null)
            {
                Debug.LogError("base_link not found!");
                enabled = false;
                return;
            }

            var base_link_ab = base_link.GetComponent<ArticulationBody>();
            var base_link_rb = base_link.GetComponent<Rigidbody>();
            body = new MixedBody(base_link_ab, base_link_rb);
            if (!body.isValid)
            {
                Debug.LogError("Base link doesnt have a valid Rigidbody or ArticulationBody.");
                enabled = false;
                return;
            }

        }

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
                if(controller.controllerType == (dji.ControllerType)0 && controller.position.y < takeoffAlt - takeoffError){
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