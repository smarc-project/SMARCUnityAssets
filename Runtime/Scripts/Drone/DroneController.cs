using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneControlScripts
{
	public class DroneController : MonoBehaviour {

		//public float max_throttle = 10f;

		[Header("Controllers")]
		public PIDController altitude_controller;
		public PIDController pitch_controller;
		public PIDController roll_controller;
		public PIDController yaw_controller;

		//public float target_altitude = 1.0f;

		public Vector3 linear_velocity;
		public Vector3 angular_velocity;

		// An array holding references to the propellers 
		// where the downward forces will be applied
		private GameObject[] propellers;
		private GameObject[] propellers_act;
		private float[] propellers_forces;
		private float torque;
		public int NumPropellers = 4;

		// The body of the quadcopter on which the forces
		// will be applied (forces and torque)
		ArticulationBody quadcopterAB;

		// Use this for initialization
		void Start () {
			quadcopterAB = gameObject.GetComponent<ArticulationBody> ();
			
			quadcopterAB.centerOfMass = Vector3.zero;
			propellers = new GameObject[NumPropellers];
			propellers[0] = GameObject.Find ("propeller1_link");
			propellers[1] = GameObject.Find ("propeller2_link");
			propellers[2] = GameObject.Find ("propeller3_link");
			propellers[3] = GameObject.Find ("propeller4_link");
			propellers_act = new GameObject[NumPropellers];
			propellers_act[0] = GameObject.Find ("propeller_1");
			propellers_act[1] = GameObject.Find ("propeller_2");
			propellers_act[2] = GameObject.Find ("propeller_3");
			propellers_act[3] = GameObject.Find ("propeller_4");

            propellers_forces = new float[NumPropellers];
			
		}
		
		// Update is called once per frame
		void FixedUpdate () {
			ComputeForcesTorque ();
			ApplyForcesTorque ();
		}

		public void UpdateVelocities(Vector3 linear_velocity, Vector3 angular_velocity) {
			this.linear_velocity = linear_velocity;
			this.angular_velocity = angular_velocity;
		}



		void ComputeForcesTorque() {

			// 1/4 of the gravity of compensated by each of the propellers
			for(int i = 0 ; i < NumPropellers ; ++i)
				propellers_forces[i] = quadcopterAB.mass * Physics.gravity.magnitude/(float)NumPropellers;
			

			Vector3 vel_in_world = quadcopterAB.linearVelocity;
			Vector3 vel_in_body = transform.InverseTransformDirection (vel_in_world);

			// Add/Decrease the throttle to keep a target vertical velocity
			// the velocity is measured along the vertical axis in the world coordinates
			// not the local vertical axis of the rigidbody
			float command_altitude = altitude_controller.Update(linear_velocity.y - vel_in_world.y, Time.fixedDeltaTime);
			for (int i = 0; i < NumPropellers; ++i)
				propellers_forces [i] += command_altitude /(float)NumPropellers;



			float command_pitch = pitch_controller.Update (linear_velocity.z - vel_in_body.z, Time.fixedDeltaTime);
			//Debug.Log (vel_in_body + "target : " + linear_velocity + " command pitch : "+ command_pitch);
			// Apply the pitch to the front propellers
			propellers_forces[0] -= command_pitch/(float)NumPropellers;
			propellers_forces[1] -= command_pitch/(float)NumPropellers;
			// And the opposite to the back
			propellers_forces[2] += command_pitch/(float)NumPropellers;
			propellers_forces[3] += command_pitch/(float)NumPropellers;


			float command_roll = roll_controller.Update (linear_velocity.x - vel_in_body.x, Time.fixedDeltaTime);
			//Debug.Log(linear_velocity.z + " ; " +  quadcopterRB.velocity.z);
			// Apply the roll to the left propellers
			propellers_forces[0] += command_roll/(float)NumPropellers;
			propellers_forces[2] += command_roll/(float)NumPropellers;
			// And the opposite to the right
			propellers_forces[1] -= command_roll/(float)NumPropellers;
			propellers_forces[3] -= command_roll/(float)NumPropellers;

			// Clamp the forces to prevent negative values
			for (int i = 0; i < NumPropellers; ++i)
				if (propellers_forces [i] < 0.0f)
					propellers_forces [i] = 0.0f;
			//propellers_forces [i] = Mathf.Clamp (propellers_forces [i], 0.0f, max_throttle);

			Vector3 rot_in_world = quadcopterAB.angularVelocity;
			Vector3 rot_in_body = transform.InverseTransformDirection (rot_in_world);
			//Debug.Log (angular_velocity.y + " " + rot_in_body.y);
			float command_yaw = yaw_controller.Update (angular_velocity.y - rot_in_body.y, Time.fixedDeltaTime);
			torque = command_yaw;
		}

		void ApplyForcesTorque() {
			
			Vector3 propellerUp, propellerPos;
			float maxForce = 0.0f;
			for (int i = 0; i < NumPropellers; ++i) {
				propellerUp = propellers [i].transform.forward;
				propellerPos = propellers [i].transform.position;
				quadcopterAB.AddForceAtPosition (propellers_forces[i] * propellerUp, propellerPos);

				// For debug, to visualize the applied forces
				if (propellers_forces [i] > maxForce)
					maxForce = propellers_forces [i];
			}


			for (int i = 0; i < NumPropellers; ++i) {
				propellerUp = propellers [i].transform.forward;
				propellerPos = propellers [i].transform.position;
				Debug.DrawRay (propellerPos, propellers_forces [i] / maxForce * propellerUp, Color.red);
			}
			quadcopterAB.AddTorque (torque/100f * quadcopterAB.transform.up);
			//Debug.Log (quadcopterRB.transform.up + " " + propellers [0].transform.up);
		}

	}
}
