using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock

using SensorBattery = VehicleComponents.Sensors.Battery;
using VehicleComponents.ROS.Core;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorBattery))]
    class Battery_Pub: ROSPublisher<BatteryStateMsg, SensorBattery>
    {
        protected override void UpdateMessage()
        {
            ROSMsg.voltage = sensor.currentVoltage;
            ROSMsg.percentage = sensor.currentPercent;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}