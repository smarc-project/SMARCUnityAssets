//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Sam
{
    [Serializable]
    public class UavcanNodeStatusNamedArrayMsg : Message
    {
        public const string k_RosMessageName = "sam_msgs/UavcanNodeStatusNamedArray";
        public override string RosMessageName => k_RosMessageName;

        public UavcanNodeStatusNamedMsg[] array;

        public UavcanNodeStatusNamedArrayMsg()
        {
            this.array = new UavcanNodeStatusNamedMsg[0];
        }

        public UavcanNodeStatusNamedArrayMsg(UavcanNodeStatusNamedMsg[] array)
        {
            this.array = array;
        }

        public static UavcanNodeStatusNamedArrayMsg Deserialize(MessageDeserializer deserializer) => new UavcanNodeStatusNamedArrayMsg(deserializer);

        private UavcanNodeStatusNamedArrayMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.array, UavcanNodeStatusNamedMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.array);
            serializer.Write(this.array);
        }

        public override string ToString()
        {
            return "UavcanNodeStatusNamedArrayMsg: " +
            "\narray: " + System.String.Join(", ", array.ToList());
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
