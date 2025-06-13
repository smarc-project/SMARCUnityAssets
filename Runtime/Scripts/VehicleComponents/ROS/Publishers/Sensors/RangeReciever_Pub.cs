using UnityEngine;
using RosMessageTypes.Std; // For Float32MultiArray
using Unity.Robotics.Core; // For TimeStamp

using RangeReciever = VehicleComponents.Sensors.RangeReciever;
using VehicleComponents.ROS.Core;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(RangeReciever))]
    class RangeReciever_Pub : ROSPublisher<Float32Msg, RangeReciever>
    { 
        protected override void UpdateMessage()
        {
            
            // Assuming sensor.depth returns a single float, you can set it as a single-element array
            ROSMsg.data =  sensor.distance ; // Wrap the depth value in an array
        }
    }
}