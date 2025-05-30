using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//Addapted from https://discussions.unity.com/t/make-a-character-walk-around-randomly/83805 Tomas Barkan

namespace Evolo
{
    public class NPCController : MonoBehaviour
    {
        public float timeToChangeDirection = 5f;
        public float maxYawRate = 20f; // Maximum yaw rate in degrees per second
        private float toNextDirection;
        private float currentYawRate;
        private Rigidbody rb;

        public void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            ChangeYawRate();
        }

        private void FixedUpdate()
        {
            toNextDirection -= Time.fixedDeltaTime;

            if (toNextDirection <= 0)
            {
                ChangeYawRate();
            }

            // Apply yaw rotation
            transform.Rotate(Vector3.up, currentYawRate * Time.fixedDeltaTime);

            // Maintain forward movement while keeping Y velocity locked
            Vector3 forwardVelocity = transform.forward * 2;
            rb.linearVelocity = new Vector3(forwardVelocity.x, 0, forwardVelocity.z);
        }

        private void ChangeYawRate()
        {
            if (currentYawRate == 0)
            {
                currentYawRate = Random.Range(-maxYawRate, maxYawRate);
            }
            else
            {
                currentYawRate = 0;
            }
            toNextDirection = timeToChangeDirection;
        }
    }
}
