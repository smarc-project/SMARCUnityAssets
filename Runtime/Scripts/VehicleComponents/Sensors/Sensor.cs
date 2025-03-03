using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

using VehicleComponents.ROS.Core;

namespace VehicleComponents.Sensors
{

    public class Sensor: LinkAttachment, IROSPublishable
    {
        [Header("Sensor")]
        public float frequency = 10f;
        public bool hasNewData = false;

        protected float Period => 1.0f/frequency;
        float timeSinceLastUpdate = 0f;

        protected void OnValidate()
        {
            if(Period < Time.fixedDeltaTime)
            {
                Debug.LogWarning($"[{transform.name}] Sensor update frequency set to {frequency}Hz but Unity updates physics at {1f/Time.fixedDeltaTime}Hz. Setting sensor period to Unity's fixedDeltaTime!");
                frequency = 1f/Time.fixedDeltaTime;
            }
        }


        public bool HasNewData()
        {
            return hasNewData;
        }


        public virtual bool UpdateSensor(double deltaTime)
        {
            Debug.Log("This sensor needs to override UpdateSensor!");
            return false;
        }

        void FixedUpdate()
        {
            timeSinceLastUpdate += Time.fixedDeltaTime;
            if(timeSinceLastUpdate < Period) return;
            hasNewData = UpdateSensor(timeSinceLastUpdate);
            timeSinceLastUpdate = 0f;
        }

    }
}
