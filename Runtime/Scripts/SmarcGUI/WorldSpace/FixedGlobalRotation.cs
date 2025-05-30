using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public class FixedGlobalRotation : MonoBehaviour
    {
        public Vector3 FixedRotationVector = Vector3.zero;
        void LateUpdate()
        {
            transform.eulerAngles = FixedRotationVector;
        }
    }
}