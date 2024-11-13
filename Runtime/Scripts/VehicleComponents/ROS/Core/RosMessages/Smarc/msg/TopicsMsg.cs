//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Smarc
{
    [Serializable]
    public class TopicsMsg : Message
    {
        public const string k_RosMessageName = "smarc_msgs/Topics";
        public override string RosMessageName => k_RosMessageName;

        public const string BATTERY_TOPIC = "'core/battery'";
        public const string GPS_TOPIC = "'core/gps'";
        public const string HEADING_TOPIC = "'core/heading'";
        public const string ABORT_TOPIC = "'core/abort'";
        public const string HEARTBEAT_TOPIC = "'core/heartbeat'";
        public const string VEHICLE_READY_TOPIC = "'core/vehicle_ready'";

        public TopicsMsg()
        {
        }
        public static TopicsMsg Deserialize(MessageDeserializer deserializer) => new TopicsMsg(deserializer);

        private TopicsMsg(MessageDeserializer deserializer)
        {
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
        }

        public override string ToString()
        {
            return "TopicsMsg: ";
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