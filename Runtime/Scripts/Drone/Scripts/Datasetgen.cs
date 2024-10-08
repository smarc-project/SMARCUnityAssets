// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System;

// public class DataCollector : MonoBehaviour
// {
//     public ArticulationBody droneBody;
//     public bool collect_data = false;
//     public int numDatapoints = 10000;
//     public int dataStep = 100;

//     void Start()
//     {
//         Transform current = transform;
//         while (current.parent != null)
//         {
//             current = current.parent;
//             ArticulationBody articulationBody = current.GetComponent<ArticulationBody>();
//             if (articulationBody != null && articulationBody.name == "base_link")
//             {
//                 // Debug.Log("base_link articulation body found: " + articulationBody);
//                 droneBody = articulationBody;
//             }
//         }
//     }

//     void FixedUpdate()
//     {
//         if (collect_data)
//         {
//             System.Random RNG = new System.Random();
//             for (int i = 0; i < numDatapoints; i++)
//             {
//                 if (i % dataStep == 0)
//                 {
//                     // Get a random direction between 1 and 4
//                     int direction = RNG.Next(1, 5);

//                     // Move the object based on the random direction
//                     switch (direction)
//                     {
//                         case 1:
//                             transform.position += new Vector3(0.5f, 0, 0);  // Increase x by 0.5
//                             break;
//                         case 2:
//                             transform.position += new Vector3(-0.5f, 0, 0);  // Decrease x by 0.5
//                             break;
//                         case 3:
//                             transform.position += new Vector3(0, 0, 0.5f);  // Increase z by 0.5
//                             break;
//                         case 4:
//                             transform.position += new Vector3(0, 0, -0.5f);  // Decrease z by 0.5
//                             break;
//                         default:
//                             Debug.LogWarning("Unexpected direction value: " + direction);
//                             break;
//                     }
//                 }
//             }
//         }
//     }
// }
