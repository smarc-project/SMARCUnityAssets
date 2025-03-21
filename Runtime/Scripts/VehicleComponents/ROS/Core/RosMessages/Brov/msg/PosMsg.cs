//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Brov
{
    [Serializable]
    public class PosMsg : Message
    {
        public const string k_RosMessageName = "brov_msgs/Pos";
        public override string RosMessageName => k_RosMessageName;

        public double x;
        public double y;
        public double z;
        public double xdot;
        public double ydot;
        public double zdot;
        public double roll;
        public double pitch;
        public double yaw;
        public double rollrate;
        public double pitchrate;
        public double yawrate;

        public PosMsg()
        {
            this.x = 0.0;
            this.y = 0.0;
            this.z = 0.0;
            this.xdot = 0.0;
            this.ydot = 0.0;
            this.zdot = 0.0;
            this.roll = 0.0;
            this.pitch = 0.0;
            this.yaw = 0.0;
            this.rollrate = 0.0;
            this.pitchrate = 0.0;
            this.yawrate = 0.0;
        }

        public PosMsg(double x, double y, double z, double xdot, double ydot, double zdot, double roll, double pitch, double yaw, double rollrate, double pitchrate, double yawrate)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.xdot = xdot;
            this.ydot = ydot;
            this.zdot = zdot;
            this.roll = roll;
            this.pitch = pitch;
            this.yaw = yaw;
            this.rollrate = rollrate;
            this.pitchrate = pitchrate;
            this.yawrate = yawrate;
        }

        public static PosMsg Deserialize(MessageDeserializer deserializer) => new PosMsg(deserializer);

        private PosMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.x);
            deserializer.Read(out this.y);
            deserializer.Read(out this.z);
            deserializer.Read(out this.xdot);
            deserializer.Read(out this.ydot);
            deserializer.Read(out this.zdot);
            deserializer.Read(out this.roll);
            deserializer.Read(out this.pitch);
            deserializer.Read(out this.yaw);
            deserializer.Read(out this.rollrate);
            deserializer.Read(out this.pitchrate);
            deserializer.Read(out this.yawrate);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.x);
            serializer.Write(this.y);
            serializer.Write(this.z);
            serializer.Write(this.xdot);
            serializer.Write(this.ydot);
            serializer.Write(this.zdot);
            serializer.Write(this.roll);
            serializer.Write(this.pitch);
            serializer.Write(this.yaw);
            serializer.Write(this.rollrate);
            serializer.Write(this.pitchrate);
            serializer.Write(this.yawrate);
        }

        public override string ToString()
        {
            return "PosMsg: " +
            "\nx: " + x.ToString() +
            "\ny: " + y.ToString() +
            "\nz: " + z.ToString() +
            "\nxdot: " + xdot.ToString() +
            "\nydot: " + ydot.ToString() +
            "\nzdot: " + zdot.ToString() +
            "\nroll: " + roll.ToString() +
            "\npitch: " + pitch.ToString() +
            "\nyaw: " + yaw.ToString() +
            "\nrollrate: " + rollrate.ToString() +
            "\npitchrate: " + pitchrate.ToString() +
            "\nyawrate: " + yawrate.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
