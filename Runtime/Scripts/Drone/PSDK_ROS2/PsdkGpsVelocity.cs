using UnityEngine;
using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


namespace Drone.PSDK_ROS2
{
    public class PsdkGpsVelocity : PsdkBase<TwistStampedMsg>
    {
        protected override void UpdateMessage()
        {
            var velocity = body.velocity;
            ROSMsg.twist.linear = velocity.To<ENU>();
            ROSMsg.header.frame_id = "psdk_map_enu";
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}