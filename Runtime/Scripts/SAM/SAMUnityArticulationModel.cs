using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Force
{
    public class SAMUnityArticulationModel : MonoBehaviour, IForceModel, ISAMControl
    {
        [FormerlySerializedAs("rigidBody")] public ArticulationBody baseLink;

        public double lcg { get; set; }
        public double vbs { get; set; }

        public SAMParameters parameters { get; set; }
        public float d_rudder { get; set; }
        public float d_aileron { get; set; }
        public double rpm1 { get; set; }
        public double rpm2 { get; set; }

        public Vector3 ThrusterPosition = new(0, 0, -0.73f);

        public SAMUnityArticulationModel()
        {
            parameters = new SAMParameters();
        }

        private void Awake()
        {
            
            Debug.Log(baseLink.inertiaTensor);
        }

        public void SetRpm1(double rpm)
        {
            // this.rpm1 = Mathf.Clamp((float)rpm, -parameters.RPMMax, parameters.RPMMax);
        }

        public void SetRpm2(double rpm)
        {
            // this.rpm2 = Mathf.Clamp((float)rpm, -parameters.RPMMax, parameters.RPMMax);
        }


        public void SetRpm(double rpm1, double rpm2)
        {
            // SetRpm1(rpm1);
            // SetRpm2(rpm2);
        }

        public void SetRudderAngle(float dr)
        {
            // d_rudder = Mathf.Clamp(dr, -parameters.ThrusterAngleMax, parameters.ThrusterAngleMax);
        }

        public void SetElevatorAngle(float de)
        {
            // d_aileron = Mathf.Clamp(de, -parameters.ThrusterAngleMax, parameters.ThrusterAngleMax);
        }

        public void SetBatteryPack(double lcg)
        {
            this.lcg = lcg; //TODO 
        }

        public void SetWaterPump(float vbs)
        {
            vbs = Mathf.Clamp01(vbs); //Percentages, given as decimal
            this.vbs = 0;

        }

        public Vector3 GetTorqueDamping()
        {
            return Vector3.zero; // Uses rigidbody angular drag
        }

        public Vector3 GetForceDamping()
        {
            return Vector3.zero; // Uses rigidbody drag
        }
    }
}