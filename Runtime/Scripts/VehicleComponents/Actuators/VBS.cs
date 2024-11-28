using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace VehicleComponents.Actuators
{
    public class VBS : LinkAttachment, IPercentageActuator
    {
        [Header("VBS")][Range(0, 100)]
        public float percentage = 50f;

        [Range(0, 100)] public float resetValue = 50f;

        public float maxVolume_l = 0.250f;
        public float density = 997f; //kg/m3
        
        private float _initialMass;
        private float _maximumPos;
        private float _minimumPos;

        public void Start()
        {
            //TODO: VBS Starts at 5% in the real world.
            var xDrive = parentMixedBody.xDrive;
         //   _initialMass = parentArticulationBody.mass;
            _initialMass = density / 1000 * maxVolume_l;
            _minimumPos = xDrive.upperLimit;
            _maximumPos = xDrive.lowerLimit;
        }

        public void SetPercentage(float newValue)
        {
            percentage = Mathf.Clamp(newValue, 0, 100);
        }

        public float GetResetValue()
        {
            return resetValue;
        }

        public float GetCurrentValue()
        {
            return (1 - (mixedBody.jointPosition[0]-_minimumPos)  / (_maximumPos - _minimumPos)) * 100;
        }

        public bool HasNewData()
        {
            return true;
        }

        public void FixedUpdate()
        {
            mixedBody.mass = 0.300f + _initialMass * GetCurrentValue() / 100; // Piston weight + water weight
            mixedBody.SetDriveTarget(ArticulationDriveAxis.X, Mathf.Lerp(_maximumPos, _minimumPos, percentage / 100));
        }
    }
}