using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity;


namespace M350.PSDK_ROS2
{
    public class PsdkAttitude : PsdkBase<QuaternionStampedMsg>
    {
        protected override void UpdateMessage()
        {
            var quaternion = body.transform.rotation;
            var rot_quat = Quaternion<ENU>.AngleAxis(-90f, Vector3<ENU>.up);
            ROSMsg.quaternion = quaternion.To<ENU>() * rot_quat; //Rotate by 90 degrees to align heading with front of drone
            ROSMsg.header.frame_id = "odom"; 
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}