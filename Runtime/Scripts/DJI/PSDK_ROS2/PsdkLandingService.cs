using UnityEngine;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;
using RosMessageTypes.Std;
using DefaultNamespace;
using dji;

using VehicleComponents.ROS.Core;
using Force;
using Unity.Robotics.Core;
using VehicleComponents.Sensors;

namespace M350.PSDK_ROS2
{
    public class PsdkLandingService : ROSBehaviour
    {
        protected string tf_prefix;

        bool registered = false;
        public float landingAlt = 3; 
        public float landingError = .05f;
        DJIController controller = null;
        LockedDirectionDepthSensor depthSensor = null;


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