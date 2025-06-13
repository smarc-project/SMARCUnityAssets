using UnityEngine;
using RosMessageTypes.Std;

using SensorLeak = VehicleComponents.Sensors.Leak;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorLeak))]
    class Leak_Pub: ROSPublisher<BoolMsg, SensorLeak>
    { 
        protected override void UpdateMessage()
        {
            if(sensor.leaked)
            {
                ROSMsg.data = true;
            }
            else ROSMsg.data = false;
        }
    }
}