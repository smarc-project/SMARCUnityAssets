using UnityEngine;
using Unity.Robotics.Core;
using VehicleComponents.ROS.Core;
using Force;
using RosMessageTypes.Nav;
using DefaultNamespace;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


namespace VehicleComponents.ROS.Publishers
{
    class SmarcOdom : ROSBehaviour
    {
        [Header("ROS Publisher")]
        public float frequency = 10f;
        float period => 1.0f/frequency;
        double lastUpdate = 0f;
        bool registered = false;


        OdometryMsg msg;
        MixedBody body;


        protected override void StartROS()
        {
            msg = new OdometryMsg();

            var robot = Utils.FindParentWithTag(gameObject, "robot", false);
            var base_link = Utils.FindDeepChildWithName(robot, "base_link").transform;
            if( base_link == null)
            {
                Debug.LogError("base_link not found for smarc odom.");
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
            
            if (!registered)
            {
                rosCon.RegisterPublisher<OdometryMsg>(topic);
                registered = true;
            }

            msg.header.frame_id = "map_gt";
            var robotName = Utils.FindParentWithTag(gameObject, "robot", false).name;
            msg.child_frame_id = $"{robotName}/base_link";
        }

        void FixedUpdate()
        {
            if (Clock.Now - lastUpdate < period) return;
            lastUpdate = Clock.Now;

            msg.header.stamp = new TimeStamp(Clock.time);
            msg.pose.pose.position = body.position.To<ENU>();
            msg.pose.pose.orientation = body.rotation.To<ENU>();

            var vel = body.transform.InverseTransformVector(body.velocity);
            msg.twist.twist.linear = vel.To<ENU>();
            var anvel = body.transform.InverseTransformVector(body.angularVelocity);
            msg.twist.twist.angular = anvel.To<ENU>();
            
            rosCon.Publish(topic, msg);
        }
    }
}
