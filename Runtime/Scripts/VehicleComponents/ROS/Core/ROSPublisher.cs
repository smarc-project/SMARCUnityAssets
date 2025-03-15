using UnityEngine;

using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;


namespace VehicleComponents.ROS.Core
{
    [RequireComponent(typeof(IROSPublishable))]
    public class ROSPublisher<RosMsgType, PublishableType> : MonoBehaviour
        where RosMsgType: ROSMessage, new()
        where PublishableType: IROSPublishable
    {
        ROSConnection ros;
        public float frequency = 10f;
        float period => 1.0f/frequency;

        // Subclasses should be able to access these
        // to get data from the sensor and put it in
        // ROSMsg as needed.
        protected PublishableType sensor;
        protected RosMsgType ROSMsg;

        [Header("ROS Publisher")]
        [Tooltip("The topic will be namespaced under the root objects name if the given topic does not start with '/'.")]
        public string topic;
        [Tooltip("If true, we will publish regardless, even if the underlying sensor says no data.")]
        public bool ignoreSensorState = false;

        protected void Start()
        {
            if(topic == null || topic == "")
            {
                Debug.LogError("ROS Publisher topic is not set!");
                enabled = false;
                return;
            }
            // We namespace the topics with the root name
            if(topic[0] != '/') topic = $"/{transform.root.name}/{topic}";

            sensor = GetComponent<PublishableType>();
            ROSMsg = new RosMsgType();

            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<RosMsgType>(topic);

            
            InitializePublication();

            InvokeRepeating("Publish", 1f, period);
        }

        protected virtual void UpdateMessage()
        {
            Debug.Log($"The ROSPublisher with topic {topic} did not override the UpdateMessage method!");
        }

        protected virtual void InitializePublication()
        {
            Debug.Log($"The ROSPublisher with topic {topic} did not override the Initialize method!");
        }

        void Publish()
        {
            // If the underlying sensor does not have new data
            // do not publish anything.
            if(sensor.HasNewData() || ignoreSensorState)
            {
                UpdateMessage();
                ros.Publish(topic, ROSMsg);
            }
        }

    }

}