using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock

using VehicleComponents.ROS.Core;
using CameraImageSensor = VehicleComponents.Sensors.CameraImage;

namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(CameraImageSensor))]
    class CameraImageCompressed_Pub: ROSPublisher<CompressedImageMsg, CameraImageSensor>
    {
        [Header("Compressed Image")]
        [Tooltip("Jpg compression quality. 1=lowest quality")]
        [Range(1,100)]
        public int quality = 75;

        protected override void InitPublisher()
        {
            ROSMsg.format = "rgb8;jpeg compressed rgb8";
            ROSMsg.header.frame_id = sensor.linkName;
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            ROSMsg.data = ImageConversion.EncodeToJPG(sensor.image, quality);
        }
    }
}