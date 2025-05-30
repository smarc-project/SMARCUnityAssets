using UnityEngine;
using RosMessageTypes.Geographic;

using SensorGPS = VehicleComponents.Sensors.GPS;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorGPS))]
    class GeoPoint_Pub: ROSPublisher<GeoPointMsg, SensorGPS>
    { 
        void OnValidate()
        {
            ignoreSensorState = true;
        }

        protected override void UpdateMessage()
        {        
            var (_, _, lat, lon) = sensor.GetUTMLatLon();
            ROSMsg.latitude = lat;
            ROSMsg.longitude = lon;
        }
    }
}