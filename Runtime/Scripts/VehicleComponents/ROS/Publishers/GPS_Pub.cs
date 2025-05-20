using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using Utils = DefaultNamespace.Utils;

using SensorGPS = VehicleComponents.Sensors.GPS;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorGPS))]
    class GPS_Pub: ROSPublisher<NavSatFixMsg, SensorGPS>
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
            if(sensor.fix) 
            {
                ROSMsg.status.status = NavSatStatusMsg.STATUS_FIX;
                ROSMsg.latitude = sensor.lat;
                ROSMsg.longitude = sensor.lon;
                ROSMsg.altitude = sensor.alt;
            }
            else ROSMsg.status.status = NavSatStatusMsg.STATUS_NO_FIX;
        }

        
    }
}
