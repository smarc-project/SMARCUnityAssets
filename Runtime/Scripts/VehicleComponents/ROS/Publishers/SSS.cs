using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using RosMessageTypes.Smarc;

using SensorSSS = VehicleComponents.Sensors.SideScanSonar;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorSSS))]
    class SSS: SensorPublisher<SidescanMsg, SensorSSS>
    { 
        void Start()
        {
            ROSMsg.header.frame_id = sensor.linkName;
        }

        public override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.port_channel = sensor.portBuckets;
            ROSMsg.starboard_channel = sensor.strbBuckets;
        }
    }
}