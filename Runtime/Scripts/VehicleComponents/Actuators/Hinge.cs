using UnityEngine;
using VehicleComponents.ROS.Core;

namespace VehicleComponents.Actuators
{
    public class Hinge: LinkAttachment, IROSPublishable
    {
        [Header("Hinge")]
        public float angle;
        public float AngleMax = 0.2f;
        public bool reverse = false;


        void OnValidate()
        {
            if (angle > AngleMax) angle = AngleMax;
            if (angle < -AngleMax) angle = -AngleMax;
        }

        public void SetAngle(float a)
        {
            angle = Mathf.Clamp(a, -AngleMax, AngleMax);
        }

        void FixedUpdate()
        {
            int direction = reverse? -1 : 1;
            parentMixedBody.SetDriveTarget(ArticulationDriveAxis.X, direction * angle * Mathf.Rad2Deg);
        }

        public bool HasNewData()
        {
            return true;
        }
    }
}