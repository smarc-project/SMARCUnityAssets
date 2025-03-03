using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils = DefaultNamespace.Utils;

namespace Rope
{
    [RequireComponent(typeof(Collider))]
    public class RopeHook : MonoBehaviour
    {

        [Header ("Rope System")]
        [Tooltip("The winch that the hook is attached to.")]
        public GameObject WinchGO;
        [Tooltip("The pulley that is attached to the hook.")]
        public GameObject PulleyGO;


        [Header("Debug")]
        public bool DrawForces = false;

        

        bool TestRopeGrab(Collision collision)
        {
            RopeLink rl;
            if(collision.gameObject.TryGetComponent(out rl))
            {
                // we want to ignore collisions with the rope depending on the "up" direction of the
                // hook and the velocity of the collision
                // essentially, only grab the rope if it's moving in the same direction as the hook
                // and the rope is "above" the hook
                // both of these should be true if the force on the collider is
                // towards the "down" direction of the hook
                Vector3 hookUp = transform.up;
                Vector3 forceDirection = collision.impulse.normalized;

                if(DrawForces)
                {
                    Debug.DrawRay(collision.contacts[0].point, forceDirection, Color.red, 1.0f);
                    Debug.DrawRay(collision.contacts[0].point, hookUp, Color.green, 1.0f);
                }

                var dot = Vector3.Dot(forceDirection, hookUp);
                if (Mathf.Abs(dot) > 0.5f)
                {
                    return true;
                }
            }
            return false;
        }

        void OnCollisionStay(Collision collision)
        {
            if(TestRopeGrab(collision))
            {
                var rl = collision.gameObject.GetComponent<RopeLink>();
                var generator = rl.GetGenerator();
                var RLBuoys = generator.RopeContainer.GetComponentsInChildren<RopeLinkBuoy>();
                if(RLBuoys.Length == 0) return;
                var RLBuoy = RLBuoys[0];
                if(RLBuoy == null) return;
                var buoyRB = RLBuoy.gameObject.GetComponent<Rigidbody>();
                var pulley = PulleyGO.GetComponent<Pulley>();
                pulley.LoadOneRB = buoyRB;
                pulley.LoadTwoRB = generator.VehicleRopeLink.gameObject.GetComponent<Rigidbody>();
                pulley.LoadTwoAB = generator.VehicleRopeLink.gameObject.GetComponent<ArticulationBody>();
                pulley.RopeLength = generator.RopeLength;
                pulley.RopeDiameter = generator.RopeDiameter;
                pulley.Setup();
                generator.DestroyRope(keepBuoy: true);
                return; // only grab things one at a time...
            }

            RopeLinkBuoy rlb;
            if(collision.gameObject.TryGetComponent(out rlb))
            {
                // if we collide with the buoy, that means the pulley is tight
                // for sim stability, we will destroy the pulley, the buoy and the hook
                // and attach the "OtherSideOfTheRope" to the winch directly.
                var winch = WinchGO.GetComponent<Winch>();
                winch.UnSetup();
                // this could be generalized to RBs and such... but for now, we'll just do the ArticulationBody
                winch.LoadAB = rlb.OtherSideOfTheRope;
                winch.CurrentRopeSpeed = 0;
                winch.WinchSpeed = 0;
                var dist = Vector3.Distance(rlb.OtherSideOfTheRope.transform.position, winch.transform.position);
                winch.TargetLength = dist;
                winch.CurrentLength = dist;
                winch.Setup();

                var pulley = PulleyGO.GetComponent<Pulley>();
                pulley.UnSetup();
                Destroy(PulleyGO);

                Destroy(rlb.gameObject);
                Destroy(gameObject);
            }
        }

    }
}