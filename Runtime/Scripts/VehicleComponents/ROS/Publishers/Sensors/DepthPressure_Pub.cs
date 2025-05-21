using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock

using SensorPressure = VehicleComponents.Sensors.DepthPressure;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorPressure))]
    class DepthPressure_Pub: ROSPublisher<FluidPressureMsg, SensorPressure>
    { 
        protected override void InitPublisher()
        {
            ROSMsg.header.frame_id = sensor.linkName;
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.fluid_pressure = sensor.pressure;
        }
    }
}