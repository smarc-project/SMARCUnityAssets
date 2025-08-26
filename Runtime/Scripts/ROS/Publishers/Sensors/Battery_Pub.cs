using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock

using SensorBattery = VehicleComponents.Sensors.Battery;
using ROS.Core;

namespace ROS.Publishers
{
    [RequireComponent(typeof(SensorBattery))]
    class Battery_Pub: ROSPublisher<BatteryStateMsg, SensorBattery>
    {
        protected override void InitPublisher()
        {
            ROSMsg.header.frame_id = $"{frame_id_prefix}/{DataSource.linkName}";
        }

        protected override void UpdateMessage()
        {
            ROSMsg.voltage = DataSource.currentVoltage;
            ROSMsg.percentage = DataSource.currentPercent;
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}
