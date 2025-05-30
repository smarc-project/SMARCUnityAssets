using RosMessageTypes.Std;

namespace Drone.PSDK_ROS2
{
    public class PsdkAltitude : PsdkBase<Float32Msg>
    {
        protected override void UpdateMessage()
        {
            ROSMsg.data = body.transform.position.y;
        }
        
    }
}