using UnityEngine;

namespace VehicleComponents.Actuators
{
    public class Prismatic : LinkAttachment, IPercentageActuator
    {
        [Header("Position")]
        [Range(0, 100)] public float percentage = 50f;
        [Range(0, 100)] public float resetValue = 50f;
     
        private float _maximumPos;
        private float _minimumPos;
        
        public void Start()
        {
            
            var xDrive = parentMixedBody.xDrive;
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
            return (1 - (mixedBody.jointPosition[0]-_minimumPos) / (_maximumPos - _minimumPos)) * 100; 
        }
        
        public void FixedUpdate()
        {
            mixedBody.SetDriveTarget(ArticulationDriveAxis.X, Mathf.Lerp(_minimumPos, _maximumPos, percentage / 100));
        }

        public bool HasNewData()
        {
            return true;
        }

        
    }
}