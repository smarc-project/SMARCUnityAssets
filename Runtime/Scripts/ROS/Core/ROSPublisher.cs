using UnityEngine;
using ROSMessage = Unity.Robotics.ROSTCPConnector.MessageGeneration.Message;
using Unity.Robotics.Core;
using Utils = DefaultNamespace.Utils;


namespace VehicleComponents.ROS.Core
{
    [RequireComponent(typeof(IROSPublishable))]
    public abstract class ROSPublisher<RosMsgType, PublishableType> : ROSBehaviour
        where RosMsgType: ROSMessage, new()
        where PublishableType: IROSPublishable
    {
        [Header("ROS Publisher")]
        public float frequency = 10f;
        float period => 1.0f/frequency;
        double lastUpdate = 0f;

        // Subclasses should be able to access these
        // to get data from the sensor and put it in
        // ROSMsg as needed.
        protected PublishableType sensor;
        protected RosMsgType ROSMsg;

        protected string frame_id_prefix = "";

        bool registered = false;

        
        [Tooltip("If true, we will publish regardless, even if the underlying sensor says no data.")]
        public bool ignoreSensorState = false;

        protected override void StartROS()
        {
            sensor = GetComponent<PublishableType>();
            ROSMsg = new RosMsgType();
            if(!registered)
            {
                rosCon.RegisterPublisher<RosMsgType>(topic);
                registered = true;
            }
            var robotGO = Utils.FindParentWithTag(gameObject, "robot", false);
            frame_id_prefix = robotGO.name;
            InitPublisher();
        }

        /// <summary>
        /// Override this method to update the ROS message with the sensor data.
        /// This method is called in Update, so that the message can be published at a fixed frequency.
        /// </summary>
        protected abstract void UpdateMessage();

        /// <summary>
        /// Override this method to initialize the ROS message.
        /// This method is called in StartROS which is called in Start, so that the message can be published at a fixed frequency.
        /// </summary>
        protected virtual void InitPublisher(){}

        /// <summary>
        /// Publish the message to ROS.
        /// We do this in Update, so that things can be disabled and enabled at runtime.
        /// </summary>
        void Update()
        {
            if (Clock.Now - lastUpdate < period) return;
            lastUpdate = Clock.Now;
            if(!(sensor.HasNewData() || ignoreSensorState)) return;
            UpdateMessage();
            rosCon.Publish(topic, ROSMsg);
        }

    }

}
