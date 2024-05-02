using RosMessageTypes.Sam;
using Unity.Robotics.Core; // Clock

namespace DefaultNamespace
{
    public class SamLCGFeedback: Sensor<PercentStampedMsg>
    {
        ISAMControl model;
        double lcg;
        void Start()
        {
            model = robotMotionModel.GetComponent<ISAMControl>();
        }

        public override bool UpdateSensor(double deltaTime)
        {
            lcg = model.lcg;
            ros_msg.value = (float)lcg;
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName; //from sensor
            return true;
        }
    }
}