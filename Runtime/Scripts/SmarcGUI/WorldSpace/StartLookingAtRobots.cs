using UnityEngine;

namespace SmarcGUI.WorldSpace
{
	[RequireComponent( typeof(Camera) )]
	public class StartLookingAtRobots : MonoBehaviour
    {
        public float StartHeight = 10;
        void Start()
        {
            var cam = GetComponent<Camera>();
            // Find all objects tagged robot in the scene
            GameObject[] robots = GameObject.FindGameObjectsWithTag("robot");

            // Find the center of all these objects
            Vector3 center = Vector3.zero;
            foreach (GameObject robot in robots)
            {
                center += robot.transform.position;
            }
            if (robots.Length > 0)
            {
                center /= robots.Length;
            }

            // Put the camera 10m above this point, looking down
            cam.transform.position = center + new Vector3(0, StartHeight, 0);
            cam.transform.LookAt(center);
        }
    }
}