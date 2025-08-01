using UnityEngine;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;
using RosMessageTypes.Std;
using DefaultNamespace;
using dji;

using VehicleComponents.ROS.Core;
using Force;
using Unity.Robotics.Core;

namespace M350.PSDK_ROS2
{
    public class PsdkReleaseControlService : ROSBehaviour
    {
        protected string tf_prefix;

        double lastUpdate = 0f;
        bool registered = false;
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
                rosCon.ImplementService<TriggerRequest, TriggerResponse>(topic, _take_control_callback);
                registered = true;
            }
        }

        private TriggerResponse _take_control_callback(TriggerRequest request){
            TriggerResponse response = new TriggerResponse();

            if(controller == null){
                controller = GetComponentInParent<DJIController>();
            }
            if(controller != null){
                controller.target_alt = controller.position.y;
                controller.controllerType = (dji.ControllerType)2; //Set to attitude control
                response.success = true;
                return response;
            }
            else{
                response.success = false;
                return response;
            }
        }

    }
}