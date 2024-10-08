using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

namespace VehicleComponents.Sensors
{
    [RequireComponent(typeof(ArticulationBody))]
    public class IMU: Sensor
    {
        // Mostly copied from https://github.com/MARUSimulator/marus-core/blob/21c003a384335777b9d9fb6805eeab1cdb93b2f0/Scripts/Sensors/Primitive/ImuSensor.cs
        // Thank you guys <3
        [Header("IMU")]
        public bool withGravity = true;
        
        [Header("Current values")]
        public Vector3 localVelocity;
        public Vector3 linearAcceleration;
        public double[] linearAccelerationCovariance = new double[9];

        public Vector3 angularVelocity;
        public double[] angularVelocityCovariance = new double[9];

        public Vector3 eulerAngles;
        public Quaternion orientation;
        public double[] orientationCovariance = new double[9];

        Vector3 lastVelocity = Vector3.zero;

        public override bool UpdateSensor(double deltaTime)
        {
            var ab = articulationBody;
            // localVelocity = ab.transform.InverseTransformVector(ab.velocity);
            localVelocity = ab.velocity;
            if (deltaTime > 0)
            {
                Vector3 deltaLinearAcceleration = localVelocity - lastVelocity;
                linearAcceleration = deltaLinearAcceleration / (float)deltaTime;
                // Debug.Log("deltaTime in IMU is " + deltaTime);
                Debug.Log("calculated acc in imu is " + linearAcceleration);
            }
            
            angularVelocity = ab.transform.InverseTransformVector(-1f * ab.angularVelocity);
            eulerAngles = ab.transform.rotation.eulerAngles;
            orientation = Quaternion.Euler(eulerAngles);
            
            lastVelocity = localVelocity;
            
            if (withGravity)
            {
                // Find the global gravity in the local frame and add to the computed linear acceleration
                Vector3 localGravity = ab.transform.InverseTransformDirection(Physics.gravity);
                linearAcceleration += localGravity;
            }
            return true;
        }
    }
}