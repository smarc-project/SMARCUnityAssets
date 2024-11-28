using UnityEngine;

using VehicleComponents.Actuators;

namespace Force
{
    
    [RequireComponent(typeof(MeshCollider))]
    public class ForceFieldPropeller : ForceFieldBase
    {
        [Header("Propeller Force Field")]
        [Tooltip("The point where the force field is applied from. This could be outside the collider bounds for more vertical pushing and inside for more horizontal pushing.")]
        public float ConeTip = 0.2f;
        [Tooltip("The propellers can spin _reall fast_ causing huge forces. Maybe cap them a little.")]
        public float ForceMagnitudeCap = 1f;
        Propeller prop;


        void Start()
        {
            prop = GetProp();
            if (prop == null)
            {
                Debug.LogWarning($"ForceFieldPropeller: No Propeller found in the parent:{transform.parent.name}, disabling propeller force field!");
                enabled = false;
            }
        }

        Propeller GetProp()
        {
            return transform.parent.GetComponent<Propeller>();
        }

        Vector3 GetTip()
        {
            return transform.position + transform.forward * ConeTip;
        }

        protected override Vector3 Field(Vector3 position)
        {
            var tip = GetTip();
            var directionToPosition = position - tip;
            var distance = directionToPosition.magnitude;
            var forceMag = (float)(prop.rpm * prop.RPMToForceMultiplier * 1/(distance*distance));
            forceMag = Mathf.Clamp(forceMag, 0, ForceMagnitudeCap);
            var dotProduct = Vector3.Dot(directionToPosition, transform.forward);
            if (dotProduct > 0)
            {
                // The position is above the tip: apply "sucktion" directly towards the tip linearly
                return forceMag * transform.forward;
            }
            else
            {
                // The position is below the tip: apply pushing force along the tip->position vector
                return forceMag * directionToPosition.normalized;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 0.2f, 0.7f, 0.5f);
            Gizmos.DrawSphere(GetTip(), 0.01f);
        }
    }


}