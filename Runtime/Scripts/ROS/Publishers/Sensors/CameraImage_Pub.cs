using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock

using ROS.Core;
using CameraImageSensor = VehicleComponents.Sensors.CameraImage;

namespace ROS.Publishers
{
    [RequireComponent(typeof(CameraImageSensor))]
    class CameraImage_Pub: ROSPublisher<ImageMsg, CameraImageSensor>
    {
        protected override void InitPublisher()
        {
            var textureHeight = DataSource.textureHeight;
            var textureWidth = DataSource.textureWidth;

            ROSMsg.data = new byte[textureHeight * textureWidth * 3];
            ROSMsg.encoding = "rgb8";
            ROSMsg.height = (uint) textureHeight;
            ROSMsg.width = (uint) textureWidth;
            ROSMsg.is_bigendian = 0;
            ROSMsg.step = (uint)(3*textureWidth);
            ROSMsg.header.frame_id = $"{frame_id_prefix}/{DataSource.linkName}";
        }

        protected override void UpdateMessage()
        {
            var img = DataSource.image.GetRawTextureData<byte>();
            for(int i=0; i<img.Length; i++) ROSMsg.data[i] = img[i]; 
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
        }
    }
}
