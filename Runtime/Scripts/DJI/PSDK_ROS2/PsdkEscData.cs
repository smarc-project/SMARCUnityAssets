using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using VehicleComponents.Sensors;
using RosMessageTypes.PsdkInterfaces;



namespace M350.PSDK_ROS2
{
    public class PsdkEscData : PsdkBase<EscDataMsg>
    {
        protected override void UpdateMessage() 
        {
            /*
            Makes a "dummyESC" that always publishes that all 4 props are at a speed of 4000. 
            Could be improved by tying to true prop speeds.
            It is necessary to publish these such that the captain knows that the drone is flying.
            */
            EscStatusIndividualMsg dummyESC = new EscStatusIndividualMsg();
            EscStatusIndividualMsg[] dummyESCs = new EscStatusIndividualMsg[4];
            dummyESC.speed = 4000;
            dummyESCs[0] = dummyESC;
            dummyESCs[1] = dummyESC;
            dummyESCs[2] = dummyESC;
            dummyESCs[3] = dummyESC;

            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.esc = dummyESCs;
        }
    }
}