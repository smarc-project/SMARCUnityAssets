using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; //Clock
using System; //Bit converter

using Sonar = VehicleComponents.Sensors.Sonar;
using VehicleComponents.ROS.Core;


namespace VehicleComponents.ROS.Publishers
{
    [RequireComponent(typeof(Sonar))]
    class SonarPointCloud_Pub: ROSPublisher<PointCloud2Msg, Sonar>
    { 
        public string frame_id="map_gt";
        protected override void InitPublisher()
        {
            ROSMsg.header.frame_id = frame_id;

            ROSMsg.height = 1; // just one long list of points
            ROSMsg.width = (uint)sensor.TotalRayCount;
            ROSMsg.is_bigendian = false;
            ROSMsg.is_dense = true;
            // 3x 4bytes (float32 x,y,z) + 1x 1byte (uint8 intensity) = 13bytes
            // Could calc this from the fields field i guess.. but meh.
            ROSMsg.point_step = 13; 
            ROSMsg.row_step = ROSMsg.width * ROSMsg.point_step;
            ROSMsg.data = new byte[ROSMsg.point_step * ROSMsg.width];
            

            ROSMsg.fields = new PointFieldMsg[4];

            ROSMsg.fields[0] = new PointFieldMsg();;
            ROSMsg.fields[0].name = "x";
            ROSMsg.fields[0].offset = 0;
            ROSMsg.fields[0].datatype = PointFieldMsg.FLOAT32;
            ROSMsg.fields[0].count = 1;

            ROSMsg.fields[1] = new PointFieldMsg();;
            ROSMsg.fields[1].name = "y";
            ROSMsg.fields[1].offset = 4;
            ROSMsg.fields[1].datatype = PointFieldMsg.FLOAT32;
            ROSMsg.fields[1].count = 1;

            ROSMsg.fields[2] = new PointFieldMsg();;
            ROSMsg.fields[2].name = "z";
            ROSMsg.fields[2].offset = 8;
            ROSMsg.fields[2].datatype = PointFieldMsg.FLOAT32;
            ROSMsg.fields[2].count = 1;

            ROSMsg.fields[3] = new PointFieldMsg();;
            ROSMsg.fields[3].name = "intensity";
            ROSMsg.fields[3].offset = 12;
            ROSMsg.fields[3].datatype = PointFieldMsg.UINT8;
            ROSMsg.fields[3].count = 1;
        }

        protected override void UpdateMessage()
        {
            ROSMsg.header.stamp = new TimeStamp(Clock.time);
            for(int i=0; i<sensor.SonarHits.Length; i++)
            {
                byte[] pointByte = sensor.SonarHits[i].GetBytes();
                Buffer.BlockCopy(pointByte, 0, ROSMsg.data, i*pointByte.Length, pointByte.Length);
            }

        }
    }
}