using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using Utils = DefaultNamespace.Utils;

using SensorPressure = VehicleComponents.Sensors.DepthPressure;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorPressure))]
    class DepthPressure_Pub: ROSPublisher<FluidPressureMsg, SensorPressure>
    { 
        protected override void InitPublisher()
        {
            var robotGO = Utils.FindParentWithTag(gameObject, "robot", false);
            string prefix = robotGO.name;
            ROSMsg.header.frame_id = $"{prefix}/{sensor.linkName}";
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.fluid_pressure = sensor.pressure;
        }
    }
}
