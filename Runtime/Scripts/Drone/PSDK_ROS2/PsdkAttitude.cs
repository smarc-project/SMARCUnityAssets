using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


namespace Drone.PSDK_ROS2
{
    public class PsdkAttitude : PsdkBase<QuaternionStampedMsg>
    {
        protected override void UpdateMessage()
        {
            var quaternion = body.transform.rotation;
            ROSMsg.quaternion = quaternion.To<ENU>();
            ROSMsg.header.frame_id = "psdk_map_enu";
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}