using RosMessageTypes.Std;
using System;
using VehicleComponents.ROS.Core;

public class TestPublisher : ROSBehaviour
{
    protected override void StartROS()
    {
        rosCon.RegisterPublisher<Float64Msg>(topic);
        InvokeRepeating("PublishTestMessage", 0.02f, 0.02f);
    }

    void PublishTestMessage()
    {
        Float64Msg testMsg = new Float64Msg
        {
            data = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000.0
        };
        rosCon.Publish(topic, testMsg);
    }
}
