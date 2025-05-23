using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using VehicleComponents.Sensors;


namespace Drone.PSDK_ROS2
{
    [RequireComponent(typeof(IMU))]
    public class PsdkAngularRate : PsdkBase<Vector3StampedMsg>
    {
        IMU imu;

        protected override void UpdateMessage()
        {
            if(imu == null) imu = GetComponent<IMU>();

            ROSMsg.vector = imu.angularVelocity.To<ENU>();
            ROSMsg.header.frame_id = "psdk_map_enu";
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}