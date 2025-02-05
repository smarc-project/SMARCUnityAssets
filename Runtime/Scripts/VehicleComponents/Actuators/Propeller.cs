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
        public double rpm;
        public float RPMMax = 100000;
        public float RPMToForceMultiplier = 0.005f;

        [Header("Drone Propeller")]
        [Tooltip("Tick it for Drone and off for SAM/ROV")]
        public bool HoverDefault = false;
        public float NumPropellers = 4f;
        [Tooltip("should there be a torque")]
        public bool ApplyTorque = false;
        [Tooltip("direction of torque")]
        public bool TorqueUp = false;
        public double DefaultHoverRPM;

        //[SerializeField] private ArticulationBody baseLinkArticulationBody;
        [SerializeField] private MixedBody baseLinkMixedBody;  // Use MixedBody instead of ArticulationBody

        private float c_tau_f = 8.004e-4f;
        
        
        public void SetRpm(double rpm)
        {
            this.rpm = Mathf.Clamp((float)rpm, -RPMMax, RPMMax);
            //if(hoverdefault) Debug.Log("setting rpm to: " + rpm);
        }
        
        void Start()
        {
            Transform current = transform;
            while (current.parent != null)
            {
                current = current.parent;
                ArticulationBody articulationBody = current.GetComponent<ArticulationBody>();
                Rigidbody rigidBody = current.GetComponent<Rigidbody>();

                /*if (articulationBody != null && articulationBody.name == "base_link")
                {
                   // Debug.Log("base_link articulation body found: " + articulationBody);
                    baseLinkArticulationBody = articulationBody;
                }*/
                if ((articulationBody != null || rigidBody != null) && current.name == "base_link" ) //current, articulatedBody and RigidBody should all have same name
                {
                    baseLinkMixedBody = new MixedBody(articulationBody, rigidBody);
                    break;
                }
            }
            if(HoverDefault) InitializeRPMToStayAfloat();
        }

        void FixedUpdate()
        {
            var r = (float)rpm * RPMToForceMultiplier;
            // if(HoverDefault) Debug.Log("the value of 4xr is: " + r*4 );

            // Visualize the applied force
            
            parentMixedBody.AddForceAtPosition((float)r * parentMixedBody.transform.forward,
                                                   parentMixedBody.transform.position,
                                                   ForceMode.Force);
            
            // Dont spin the props (which lets physics handle the torques and such) if we are applying manual
            // torque. This is useful for drones or vehicles where numerical things are known
            // and simulation is not wanted.
            if(ApplyTorque)   
            {
                int torque_sign = TorqueUp ? 1 : -1;
                float torque = torque_sign * c_tau_f * (float)r;
                Vector3 torqueVector = torque * transform.forward;
                parentMixedBody.AddTorque(torqueVector, ForceMode.Force);
            }
            else
            {
                int direction = reverse? -1 : 1;
                parentMixedBody.SetDriveTargetVelocity(ArticulationDriveAxis.X, direction*(float)rpm);
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