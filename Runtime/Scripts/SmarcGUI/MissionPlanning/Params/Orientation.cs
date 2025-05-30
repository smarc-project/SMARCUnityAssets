using Newtonsoft.Json;
using UnityEngine;
using System;


namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct Orientation
    {
        // A quaternion that corresponds to the X-forward Y-left Z-up coordinate system
        // used in ROS!
        public double w{get; set;}
        public double x{get; set;}
        public double y{get; set;}
        public double z{get; set;}

        static double Deg2Rad = Math.PI / 180.0;
        static double Rad2Deg = 180.0 / Math.PI;


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
            double exRad = Deg2Rad * ex;
            double eyRad = Deg2Rad * ey;
            double ezRad = Deg2Rad * ez;

            double cy = Math.Cos(ezRad * 0.5f);
            double sy = Math.Sin(ezRad * 0.5f);
            double cp = Math.Cos(eyRad * 0.5f);
            double sp = Math.Sin(eyRad * 0.5f);
            double cr = Math.Cos(exRad * 0.5f);
            double sr = Math.Sin(exRad * 0.5f);

            w = cr * cp * cy + sr * sp * sy;
            x = sr * cp * cy - cr * sp * sy;
            y = cr * sp * cy + sr * cp * sy;
            z = cr * cp * sy - sr * sp * cy;
        }

        private static double CopySign(double magnitude, double sign)
        {
            return Math.Abs(magnitude) * Math.Sign(sign);
        }

        public Vector3 ToRPY()
        {
            
            double ex, ey, ez;

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (w * x + y * z);
            double cosr_cosp = 1 - 2 * (x * x + y * y);
            ex = Math.Atan2(sinr_cosp, cosr_cosp)* Rad2Deg;

            // pitch (y-axis rotation)
            double sinp = 2 * (w * y - z * x);
            if (Math.Abs(sinp) >= 1)
            {
                ey = CopySign(Math.PI / 2, sinp)* Rad2Deg;
            }
            else
            {
                ey = Math.Asin(sinp)* Rad2Deg;
            }

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (w * z + x * y);
            double cosy_cosp = 1 - 2 * (y * y + z * z);
            ez = Math.Atan2(siny_cosp, cosy_cosp)* Rad2Deg;

            var angles = new Vector3((float)ex, (float)ey, (float)ez);

            return angles;
        }
    }
}