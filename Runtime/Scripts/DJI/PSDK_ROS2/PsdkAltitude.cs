using RosMessageTypes.Std;

namespace M350.PSDK_ROS2
{
    public class PsdkAltitude : PsdkBase<Float32Msg>
    {
        protected override void UpdateMessage()
        {
            ROSMsg.data = body.transform.position.y;
        }
        
    }
}