using DefaultNamespace;
using UnityEngine;

namespace Force
{
    public class DefaultForceModel : MonoBehaviour, IForceModel
    {
        private Rigidbody _rigidbody;
        public float waterDrag = 0.5f;
        public float waterAngularDrag = 0.5f;
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {

            _rigidbody.AddForce(-_rigidbody.linearVelocity
                                * waterDrag, ForceMode.Acceleration);
            _rigidbody.AddTorque(-_rigidbody.angularVelocity
                                 * waterAngularDrag, ForceMode.Acceleration);

        }
        public Vector3 GetTorqueDamping()
        {
            return Vector3.zero;
        }
        public Vector3 GetForceDamping()
        {
            return Vector3.zero;
        }
    }
}
