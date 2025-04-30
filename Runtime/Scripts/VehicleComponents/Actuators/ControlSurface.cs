using UnityEngine;
using Force;

namespace VehicleComponents.Actuators
{

    public enum ControlSurfaceOrientation
    {
        Vertical,
        Horizontal
    }

    [RequireComponent(typeof(LinkAttachment))]
    public class ControlSurface : MonoBehaviour
    {
    
        [Header("Control Surface")]
        [Tooltip("The force multiplier for the control surface. In place of knowing coefficients, areas etc.")]
        public float LiftForceMultiplier = 1.0f;
        public float DragForceMultiplier = 1.0f;

        [Tooltip("The orientation of the control surface.")]
        public ControlSurfaceOrientation Orientation = ControlSurfaceOrientation.Horizontal;



        MixedBody mixedBody;

        void Start()
        {
            var la = GetComponent<LinkAttachment>();
            mixedBody = la.GetMixedBody();
            if (!mixedBody.isValid)
            {
                Debug.LogError("ControlSurface requires a Rigidbody or ArticulationBody component! Disabling.");
                enabled = false;
            }
        }

        void FixedUpdate()
        {
            var velocity = mixedBody.velocity;
            
            var sideVec = Orientation switch
            {
                ControlSurfaceOrientation.Horizontal => transform.right,
                ControlSurfaceOrientation.Vertical => transform.up,
                _ => throw new System.ArgumentOutOfRangeException(),
            };

            var positiveVec = Orientation switch
            {
                ControlSurfaceOrientation.Horizontal => transform.up,
                ControlSurfaceOrientation.Vertical => -transform.right,
                _ => throw new System.ArgumentOutOfRangeException(),
            };

            var angleOfAttack = Vector3.SignedAngle(transform.forward, velocity, sideVec);
            
            var liftForce = LiftForceMultiplier * Mathf.Sin(angleOfAttack * Mathf.Deg2Rad) * positiveVec;
            var dragForce = DragForceMultiplier * Mathf.Cos(angleOfAttack * Mathf.Deg2Rad) * -transform.forward;

            var velMag = velocity.magnitude;
            var velMagSq = velMag * velMag;
            liftForce *= velMagSq;
            dragForce *= velMagSq;
            
            mixedBody.AddForceAtPosition(liftForce, transform.position, ForceMode.Force);
            mixedBody.AddForceAtPosition(dragForce, transform.position, ForceMode.Force);

            // Debug.DrawRay(transform.position, liftForce, Color.green, 0.1f);
            // Debug.DrawRay(transform.position, dragForce, Color.red, 0.1f);
        }

    }
}