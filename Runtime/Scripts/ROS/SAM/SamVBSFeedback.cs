using RosMessageTypes.Sam;
using Unity.Robotics.Core; // Clock

namespace DefaultNamespace
{
    public class SamVBSFeedback: Sensor<PercentStampedMsg>
    {
        ISAMControl model;
        double vbs;
        void Start()
        {
            model = robotMotionModel.GetComponent<ISAMControl>();
        }

        public override bool UpdateSensor(double deltaTime)
        {
            vbs = model.vbs;
            ros_msg.value = (float)vbs;
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName; //from sensor
            return true;
        }
    }
}