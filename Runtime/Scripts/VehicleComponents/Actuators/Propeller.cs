using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;
using Force;  // MixedBody is in the Force namespace


using VehicleComponents.ROS.Core;

namespace VehicleComponents.Actuators
{
    public class Propeller: LinkAttachment, IROSPublishable
    {
        [Header("Propeller")]
        public bool reverse = false;
        public float rpm;
        public float RPMMax = 100000;
        public float RPMMin = 0;
        public float RPMToForceMultiplier = 0.005f;

        [Header("Drone Propeller")]
        [Tooltip("Tick it for Drone and off for SAM/ROV")]
        public bool HoverDefault = false;
        public float NumPropellers = 4f;
        [Tooltip("should there be a torque")]
        public bool ApplyTorque = false;
        [Tooltip("direction of torque")]
        public bool TorqueUp = false;
        public float DefaultHoverRPM;

        public ArticulationBody baseLinkArticulationBody;
        public Rigidbody baseLinkRigidBody;
        private float c_tau_f = 8.004e-4f;
        private MixedBody baseLinkMixedBody; 


        void OnValidate()
        {
            // make sure the RPM is within the limits
            if (rpm > RPMMax) rpm = RPMMax;
            if (rpm < -RPMMax) rpm = -RPMMax;
        }
        
        public void SetRpm(float rpm)
        {
            if(Mathf.Abs(rpm) < RPMMin) rpm = 0;
            this.rpm = Mathf.Clamp(rpm, -RPMMax, RPMMax);
            //if(hoverdefault) Debug.Log("setting rpm to: " + rpm);
        }
        
        void Start()
        {
            baseLinkMixedBody = new MixedBody(baseLinkArticulationBody, baseLinkRigidBody);
            if(HoverDefault) InitializeRPMToStayAfloat();
        }

        void FixedUpdate()
        {
            float r = rpm * RPMToForceMultiplier;
            // if(HoverDefault) Debug.Log("the value of 4xr is: " + r*4 );

            // Visualize the applied force
            
            parentMixedBody.AddForceAtPosition(r * parentMixedBody.transform.forward,
                                                   parentMixedBody.transform.position,
                                                   ForceMode.Force);
            
            // Dont spin the props (which lets physics handle the torques and such) if we are applying manual
            // torque. This is useful for drones or vehicles where numerical things are known
            // and simulation is not wanted.
            if(ApplyTorque)   
            {
                int torque_sign = TorqueUp ? 1 : -1;
                float torque = torque_sign * c_tau_f * r;
                Vector3 torqueVector = torque * transform.forward;
                parentMixedBody.AddTorque(torqueVector, ForceMode.Force);
            }
            else
            {
                int direction = reverse? -1 : 1;
                parentMixedBody.SetDriveTargetVelocity(ArticulationDriveAxis.X, direction*rpm);
            }
        }

        private void InitializeRPMToStayAfloat()
        {
            // Calculate the required force to counteract gravity
            //float requiredForce = baseLinkArticulationBody.mass * Physics.gravity.magnitude;
            float requiredForce = baseLinkMixedBody.mass * Physics.gravity.magnitude;

            // Debug.Log("Required force to stay afloat: " + requiredForce);

            // Calculate the required RPM for each propeller
            float requiredForcePerProp = requiredForce/NumPropellers;
            float requiredRPM = requiredForcePerProp / RPMToForceMultiplier;
            DefaultHoverRPM = requiredRPM;

            // Set the initial RPM to each propeller
            SetRpm(requiredRPM);
        }

        public bool HasNewData()
        {
            return true;
        }
        
    }
}
