using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

using SensorIMU = VehicleComponents.Sensors.IMU;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorIMU))]
    class IMU_Pub: ROSPublisher<ImuMsg, SensorIMU>
    { 
        [Tooltip("If false, orientation is in ENU in ROS.")]
        public bool useNED = false;
        protected override void InitializePublication()
        {
            ROSMsg.header.frame_id = sensor.linkName;
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            if(useNED) ROSMsg.orientation = sensor.orientation.To<NED>();
            else ROSMsg.orientation = sensor.orientation.To<ENU>();
            ROSMsg.angular_velocity = sensor.angularVelocity.To<FLU>();
        
            ROSMsg.linear_acceleration = sensor.linearAcceleration.To<FLU>();

        }
    }
}