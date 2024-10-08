using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // For file operations
using Utils = DefaultNamespace.Utils;
using DefaultNamespace.Water;

namespace VehicleComponents.Sensors
{
    public class DepthSensor : Sensor
    {
        [Header("Depth-Sensor")]
        public float depth;
        private WaterQueryModel _waterModel;
        private bool headerWritten = false;
        public float variance = 0.001f;

        void Start()
        {
            _waterModel = FindObjectsByType<WaterQueryModel>(FindObjectsSortMode.None)[0];
            depth = 0f;
        }

        public override bool UpdateSensor(double deltaTime)
        {
            float maxRaycastDistance = 30f;  // Adjust based on your needs
            RaycastHit hit;

            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = Vector3.down;

            // Perform raycast downwards from the current position
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRaycastDistance))
            {
                // If raycast hits something, use the hit point's y-coordinate
                Debug.Log("Raycast hit at y: " + hit.point.y);
                depth = -(hit.point.y - transform.position.y);
            }
            else
            {
                // If no hit, fall back to water level calculation
                float waterSurfaceLevel = _waterModel.GetWaterLevelAt(transform.position);
                // Debug.Log("y: " + transform.position.y);
                depth = -(waterSurfaceLevel - transform.position.y);
            }
            //Add gaussian noise
            float noise = GenerateGaussianNoise(0f, variance);
            depth = depth*(1 + noise);
            return true;
        } 

        private float GenerateGaussianNoise(float mean = 0f, float stdDev = 1f)
        {
            float u1 = 1.0f - Random.value;
            float u2 = 1.0f - Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        } 
    }
}
