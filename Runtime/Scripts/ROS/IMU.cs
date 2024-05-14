using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.Core; // Clock
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace DefaultNamespace
{
    public class IMU : Sensor<ImuMsg>
    {
        // Mostly copied from https://github.com/MARUSimulator/marus-core/blob/21c003a384335777b9d9fb6805eeab1cdb93b2f0/Scripts/Sensors/Primitive/ImuSensor.cs
        [Header("IMU")]
        public bool withGravity = true;

        public bool useNED = false;
        
        Vector3 linearAcceleration;
        Vector3 localVelocity;
        double[] linearAccelerationCovariance = new double[9];

        Vector3 angularVelocity;
        double[] angularVelocityCovariance = new double[9];

        Vector3 eulerAngles;
        Quaternion orientation;
        double[] orientationCovariance = new double[9];

        Vector3 lastVelocity = Vector3.zero;

        public override bool UpdateSensor(double deltaTime)
        {
            //TODO add noise to localVel, angularVel, eulerAngles [0],[1],[2]
            localVelocity = rb.transform.InverseTransformVector(rb.velocity);
            if (deltaTime > 0)
            {
                Vector3 deltaLinearAcceleration = localVelocity - lastVelocity;
                linearAcceleration = deltaLinearAcceleration / (float)deltaTime;
            }
            
            angularVelocity = -1f * rb.angularVelocity;
            eulerAngles = rb.rotation.eulerAngles;
            orientation = Quaternion.Euler(eulerAngles);

            lastVelocity = localVelocity;

            if (withGravity)
            {
                // Find the global gravity in the local frame and add to the computed linear acceleration
                Vector3 localGravity = rb.transform.InverseTransformDirection(Physics.gravity);
                linearAcceleration += localGravity;
            }
                

            ros_msg.header.stamp = new TimeStamp(Clock.time);
            ros_msg.header.frame_id = robotLinkName; //from sensor

            if(useNED)
            {
                ros_msg.orientation = orientation.To<NED>();
            }
            else
            {
                ros_msg.orientation = orientation.To<ENU>();
            }

            // ros_msg.orientation_covariance = orientationCovariance;

            ros_msg.angular_velocity =  angularVelocity.To<FLU>();

            // ros_msg.angular_velocity_covariance = angularVelocityCovariance;

            ros_msg.linear_acceleration = linearAcceleration.To<FLU>();

            // ros_msg.linear_acceleration_covariance = linearAccelerationCovariance;
            return true;
        }
    }
}