using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // For file operations
using Utils = DefaultNamespace.Utils;
using DefaultNamespace.Water;

namespace VehicleComponents.Sensors
{
    public class RangeReciever : Sensor
    {
        [Header("Range-Reciever")]
        public float distance;
        public GameObject senderObject;
        public float range = 10f;
        public float variance = 0.001f;

        private bool headerWritten = false;

        void Start()
        {
            
            distance = float.PositiveInfinity;
        }

        public override bool UpdateSensor(double deltaTime)
        {
            float maxRaycastDistance = 30f;  // Adjust based on your needs
            RaycastHit hit;

            Vector3 rayOrigin = senderObject.transform.position;
            Vector3 rayEnd = transform.position;
            
            // Perform raycast downwards from the current position
            if (senderObject != null && Vector3.Distance(rayOrigin,rayEnd) < range)
            {   
                float noise = GenerateGaussianNoise(0f, variance);
                distance = Vector3.Distance(rayOrigin,rayEnd)*(1 + noise);
                // If raycast hits something, use the hit point's y-coordinate
                Debug.Log("Distance of AUV: " + distance);
            }
            else
            {
                distance = float.PositiveInfinity;
                Debug.Log("Either senderObject is null or the sensors are not in range");
            }

            return true;
        }

                // Gaussian noise helper function
        private float GenerateGaussianNoise(float mean = 0f, float stdDev = 1f)
        {
            float u1 = 1.0f - Random.value;
            float u2 = 1.0f - Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        } 
    }
}

