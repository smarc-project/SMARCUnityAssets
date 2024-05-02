using UnityEngine;
using RosMessageTypes.Smarc;
using Unity.Robotics.Core; // Clock

namespace DefaultNamespace
{
    public class SamThrusterFeedback : Sensor<ThrusterFeedbackMsg>
    {
        [Header("Thruster FB")]
        [Tooltip("Set the number (1 or 2) of the thruster the feedback will come from")]
        [Range(1,2)]
        public int thrusterNum;
        ISAMControl model;
        double rpm;
        void Start()
        {
            model = robotMotionModel.GetComponent<ISAMControl>();
        }

        public override bool UpdateSensor(double deltaTime)
        {
            rpm = thrusterNum==1 ? model.rpm1 : model.rpm2;
            ros_msg.rpm.rpm = (int)rpm;
            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName; //from sensor
            return true;
        }
    }
}