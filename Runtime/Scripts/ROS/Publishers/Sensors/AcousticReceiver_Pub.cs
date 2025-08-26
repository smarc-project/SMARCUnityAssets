
using UnityEngine;

using Unity.Robotics.Core; //Clock

using RosMessageTypes.Smarc; // StringStampedMsg
using TX = VehicleComponents.Acoustics.Transceiver;
using StringStamped = VehicleComponents.Acoustics.StringStamped;
using VehicleComponents.ROS.Core;

namespace VehicleComponents.ROS.Publishers
{

    [RequireComponent(typeof(TX))]
    public class AcousticReceiver_Pub : ROSPublisher<StringStampedMsg, TX>
    {
        protected override void UpdateMessage()
        {
            StringStamped dp = sensor.Read();
            if(dp == null) return;
            ROSMsg.data = dp.Data;
            ROSMsg.time_sent = new TimeStamp(dp.TimeSent);
            ROSMsg.time_received = new TimeStamp(dp.TimeReceived);
        }   
    }
}