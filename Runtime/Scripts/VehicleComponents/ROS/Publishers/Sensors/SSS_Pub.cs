using UnityEngine;
using System;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using RosMessageTypes.Smarc;

using SideScanSonar = VehicleComponents.Sensors.Sonar;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SideScanSonar))]
    class SSS_Pub: ROSPublisher<SidescanMsg, SideScanSonar>
    { 
        protected override void InitPublisher()
        {
            ROSMsg.header.frame_id = sensor.linkName;
            ROSMsg.port_channel = new byte[sensor.NumBucketsPerBeam];
            ROSMsg.starboard_channel = new byte[sensor.NumBucketsPerBeam];
            ROSMsg.port_channel_angle_high = new byte[sensor.NumBucketsPerBeam];
            ROSMsg.port_channel_angle_low = new byte[sensor.NumBucketsPerBeam];
            ROSMsg.starboard_channel_angle_high = new byte[sensor.NumBucketsPerBeam];
            ROSMsg.starboard_channel_angle_low = new byte[sensor.NumBucketsPerBeam];

        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            var mid = sensor.NumBucketsPerBeam;
            Array.Copy(sensor.Buckets, 0, ROSMsg.port_channel, 0, mid);
            Array.Copy(sensor.Buckets, mid, ROSMsg.starboard_channel, 0, mid);
            Array.Copy(sensor.BucketsAngleHigh, 0, ROSMsg.port_channel_angle_high, 0, mid);
            Array.Copy(sensor.BucketsAngleLow, 0, ROSMsg.port_channel_angle_low, 0, mid);
            Array.Copy(sensor.BucketsAngleHigh, mid, ROSMsg.starboard_channel_angle_high, 0, mid);
            Array.Copy(sensor.BucketsAngleLow, mid, ROSMsg.starboard_channel_angle_low, 0, mid);
        }
    }
}