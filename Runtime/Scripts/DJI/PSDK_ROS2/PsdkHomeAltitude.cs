using RosMessageTypes.Std;

namespace M350.PSDK_ROS2
{
    public class PsdkHomeAltitude : PsdkBase<Float32Msg>
    {

        float initialAlt;
        protected override void InitPublisher(){
            initialAlt = (float) body.transform.position.y;
        }
        protected override void UpdateMessage()
        {
            ROSMsg.data = initialAlt;
        }
        
    }
}