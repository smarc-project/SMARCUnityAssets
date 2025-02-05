using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;
using Force;  // Assuming MixedBody is in the Force namespace

namespace VehicleComponents.Sensors
{
    public class IMU : Sensor
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

        private Vector3 lastVelocity = Vector3.zero;

       
        public override bool UpdateSensor(double deltaTime)
        {
            if (!mixedBody.isValid)
            {
                Debug.LogError("No valid body found for IMU!");
                return false;
            }

            // Use MixedBody to handle both Rigidbody and ArticulationBody
            localVelocity = mixedBody.transform.InverseTransformVector(mixedBody.ab ? mixedBody.velocity : mixedBody.velocity);

            if (deltaTime > 0)
            {
                Vector3 deltaLinearAcceleration = localVelocity - lastVelocity;
                linearAcceleration = deltaLinearAcceleration / (float)deltaTime;
            }

            angularVelocity = mixedBody.transform.InverseTransformVector(mixedBody.angularVelocity);
            eulerAngles = mixedBody.transform.rotation.eulerAngles;
            orientation = Quaternion.Euler(eulerAngles);

            lastVelocity = localVelocity;

            if (withGravity)
            {
                // Find the global gravity in the local frame and add to the computed linear acceleration
                Vector3 localGravity = mixedBody.transform.InverseTransformDirection(Physics.gravity);
                linearAcceleration += localGravity;
            }

            return true;
        }
    }
}
