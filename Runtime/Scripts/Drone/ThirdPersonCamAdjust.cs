using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamAdjust : MonoBehaviour {
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 hf = transform.parent.forward;
		hf.y = 0;
		hf.Normalize();
		Vector3 pos = transform.parent.position + 0.5f * transform.parent.up - 1.0f * hf ;
		//current_pos.y = transform.parent.position.y + 4.0f;
		transform.position = pos;
		transform.LookAt (transform.parent.position);
	}
}
