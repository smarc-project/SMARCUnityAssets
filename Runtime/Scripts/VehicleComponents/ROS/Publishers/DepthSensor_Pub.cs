using UnityEngine;
using RosMessageTypes.Std; // For Float32MultiArray
using Unity.Robotics.Core; // For TimeStamp

using SensorDepth = VehicleComponents.Sensors.DepthSensor;
using VehicleComponents.ROS.Core;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(SensorDepth))]
    class DepthSensor_Pub : ROSPublisher<Float32Msg, SensorDepth>
    { 
        // protected override void InitializePublication()
        // {
        //     // Initialize the header with the link name
        //     // ROSMsg.header.frame_id = sensor.linkName;
        // }

        protected override void UpdateMessage()
        {
            
            // Assuming sensor.depth returns a single float, you can set it as a single-element array
            ROSMsg.data =  sensor.depth ; // Wrap the depth value in an array
        }
    }
}
