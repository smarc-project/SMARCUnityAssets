using UnityEngine;
using RosMessageTypes.Std;
using SensorDepth = VehicleComponents.Sensors.LockedDirectionDepthSensor;
using ROS.Core;

namespace ROS.Publishers
{
    [RequireComponent(typeof(SensorDepth))]
    class LockedDirectionDepthSensor_Pub : ROSPublisher<Float32Msg, SensorDepth>
    { 

        protected override void UpdateMessage()
        {
            ROSMsg.data =  DataSource.depth ;
        }
    }
}
