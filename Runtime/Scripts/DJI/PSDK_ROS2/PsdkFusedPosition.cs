using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using VehicleComponents.Sensors;


namespace M350.PSDK_ROS2
{
    public class PsdkFusedPos : PsdkBase<Vector3StampedMsg>
    {
        Vector3 initialPos;
        protected override void InitPublisher(){
            initialPos.x = (float)body.transform.position.z;
            initialPos.y = (float)body.transform.position.x;
            initialPos.z = (float)body.transform.position.y;
        }

        protected override void UpdateMessage()
        {
            Vector3 currPos;
            currPos.x = (float)body.transform.position.z;
            currPos.y = (float)body.transform.position.x;
            currPos.z = (float)body.transform.position.y;
            currPos = currPos - initialPos;

            ROSMsg.vector.x = currPos.x;
            ROSMsg.vector.y = currPos.y;
            ROSMsg.vector.z = currPos.z;
            ROSMsg.header.frame_id = "psdk_map_enu";
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}