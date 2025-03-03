using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStabilizer : MonoBehaviour {

    public bool StabilizeCamera = true;

    // Update is called once per frame
    void Update () {
        if (StabilizeCamera) {
            // We take the downward direction of the camera
            // Vector3 down = -transform.up;
			// // make it so that it points down;
			// down.x = 0;
			// down.z = 0;
            // Use this to define the look-at direction of the camera;
            transform.LookAt(transform.position + Vector3.down, transform.parent.up);
        } else {
            transform.rotation = transform.parent.rotation;
        }
    }
}
