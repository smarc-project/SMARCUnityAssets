using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//Addapted from https://discussions.unity.com/t/make-a-character-walk-around-randomly/83805 Tomas Barkan

namespace Evolo
{ 
    public class NPCController : MonoBehaviour {
        public float timeToChangeDirection = 5f;
        public float maxYawRate = 20f; // Maximum yaw rate in degrees per second
        private float toNextDirection;
        private float currentYawRate;
        private Rigidbody rb;

        // Use this for initialization
        public void Start() {
            rb = GetComponent<Rigidbody>();
            ChangeYawRate();
        }
        
        // Update is called once per frame - no need for update fixed because not time critical
        public void Update() {
            toNextDirection -= Time.deltaTime;

            if (toNextDirection <= 0) {
                ChangeYawRate();
            }

            // Apply yaw rotation
            transform.Rotate(Vector3.up, currentYawRate * Time.deltaTime);

            // Maintain forward movement
            Vector3 forwardVelocity = transform.forward * 2;
            rb.velocity = new Vector3(forwardVelocity.x, 0, forwardVelocity.z);
            
        }

        private void ChangeYawRate() {
            if(currentYawRate==0){ //we want to move in a straight line and curving alternating
            currentYawRate = Random.Range(-maxYawRate, maxYawRate); // Random yaw rate
            toNextDirection = timeToChangeDirection;
        } else{
            currentYawRate=0;
            toNextDirection = timeToChangeDirection;

        }
        }
    }
}