using Newtonsoft.Json;
using UnityEngine;
using System;

// FIXME THE MATH HERE IS NOT CORRECT
// RPY -> ORI -> RPY DOES NOT PRODUCE THE SAME RPY !!
namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct Orientation
    {
        // A quaternion that corresponds to the X-forward Y-left Z-up coordinate system
        // used in ROS!
        public float w{get; set;}
        public float x{get; set;}
        public float y{get; set;}
        public float z{get; set;}


        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Orientation(string json)
        {
            var o = JsonConvert.DeserializeObject<Orientation>(json);
            w = o.w;
            x = o.x;
            y = o.y;
            z = o.z;
        }

        // given roll, pitch yaw in the order of x, y, z
        // where roll is around the x axis, pitch is around the y axis and yaw is around the z axis
        // the quaternion is given by:
        // q = [cos(roll/2)cos(pitch/2)cos(yaw/2) - sin(roll/2)sin(pitch/2)sin(yaw/2),
        //      sin(roll/2)cos(pitch/2)cos(yaw/2) + cos(roll/2)sin(pitch/2)sin(yaw/2),
        //      cos(roll/2)sin(pitch/2)cos(yaw/2) - sin(roll/2)cos(pitch/2)sin(yaw/2),
        //      cos(roll/2)cos(pitch/2)sin(yaw/2) + sin(roll/2)sin(pitch/2)cos(yaw/2)]
        // where roll, pitch and yaw are in radians
        // and the quaternion is given by:
        // q = [qx, qy, qz, qw]
        public Orientation(float ex, float ey, float ez)
        {
            // In X forward, Y left, Z up coordinate system!
            // roll = around x
            // pitch = around y
            // yaw = around z
            float exRad = Mathf.Deg2Rad * ex;
            float eyRad = Mathf.Deg2Rad * ey;
            float ezRad = Mathf.Deg2Rad * ez;

            float cy = Mathf.Cos(ezRad * 0.5f);
            float sy = Mathf.Sin(ezRad * 0.5f);
            float cp = Mathf.Cos(eyRad * 0.5f);
            float sp = Mathf.Sin(eyRad * 0.5f);
            float cr = Mathf.Cos(exRad * 0.5f);
            float sr = Mathf.Sin(exRad * 0.5f);

            w = cr * cp * cy + sr * sp * sy;
            x = sr * cp * cy - cr * sp * sy;
            y = cr * sp * cy + sr * cp * sy;
            z = cr * cp * sy - sr * sp * cy;
            Debug.Log($"RPY {ex}, {ey}, {ez} -> Ori {w}, {x}, {y}, {z}");
        }

        private static double CopySign(double magnitude, double sign)
        {
            return Math.Abs(magnitude) * Math.Sign(sign);
        }

        public Vector3 ToRPY()
        {
            var angles = new Vector3();

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (w * x + y * z);
            double cosr_cosp = 1 - 2 * (x * x + y * y);
            angles.x = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (w * y - z * x);
            if (Math.Abs(sinp) >= 1)
            {
                angles.y = (float)CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.y = (float)Math.Asin(sinp);
            }

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (w * z + x * y);
            double cosy_cosp = 1 - 2 * (y * y + z * z);
            angles.z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            Debug.Log($"Ori {w}, {x}, {y}, {z} -> RPY {angles.x}, {angles.y}, {angles.z}");
            return angles;
        }
    }
}