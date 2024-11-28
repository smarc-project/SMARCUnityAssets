using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DefaultNamespace; // ResetArticulationBody() extension

using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry; 
using Unity.Robotics.ROSTCPConnector;

namespace VehicleComponents.ROS.Subscribers
{
    public class Teleporter_Sub : MonoBehaviour
    {
        [Tooltip("The topic will be namespaced under the root objects name if the given topic does not start with '/'.")]
        public string topic;
        [Tooltip("The object to teleport around. Can handle Arti. Bodies too.")]
        public Transform Target;

        ROSConnection ros;

        ArticulationBody[] ABparts;
        Rigidbody[] RBparts;

        int immovableStage = 2;

        void OnValidate()
        {
            if(Target.TryGetComponent<ArticulationBody>(out ArticulationBody targetAb))
                if(!targetAb.isRoot) Debug.LogWarning($"Assigned target object is an Arti. body, but it is not the root. Non-root articulation bodies can not be teleported!");
        }

        void Start()
        {
            if(topic == null) return;
            if(topic[0] != '/') topic = $"/{transform.root.name}/{topic}";

            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<PoseMsg>(topic, UpdateMessage);

            ABparts = Target.gameObject.GetComponentsInChildren<ArticulationBody>();
            RBparts = Target.gameObject.GetComponentsInChildren<Rigidbody>();
        }


        void UpdateMessage(PoseMsg pose)
        {
            // if its an articulation body, we need to use a specific method
            // otherwise just setting local position/rotation is enough.
            var unityPosi = FLU.ConvertToRUF(
                        new Vector3(
                            (float)pose.position.x,
                            (float)pose.position.y,
                            (float)pose.position.z));

            var unityOri = FLU.ConvertToRUF(
                        new Quaternion(
                            (float)pose.orientation.x,
                            (float)pose.orientation.y,
                            (float)pose.orientation.z,
                            (float)pose.orientation.w));

            ArticulationBody targetAb;
            if(Target.TryGetComponent<ArticulationBody>(out targetAb))
            {
                if(!targetAb.isRoot) return;
                targetAb.immovable = true;
                immovableStage = 0;
                targetAb.TeleportRoot(unityPosi, unityOri);
            }
            else
            {
                Target.localPosition = unityPosi;
                Target.localRotation = unityOri;
            }


            foreach(var ab in ABparts)
            {
                ab.linearVelocity = Vector3.zero;
                ab.angularVelocity = Vector3.zero;
                ab.ResetArticulationBody();
            }

            foreach(var rb in RBparts)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        void FixedUpdate()
        {
            switch(immovableStage)
            {
                case 0:
                    immovableStage = 1;
                    break;
                case 1:
                    if(Target.TryGetComponent<ArticulationBody>(out ArticulationBody targetAb))
                    {
                        if(!targetAb.isRoot) return;
                        targetAb.immovable = false;
                    }
                    immovableStage = 2;
                    break;
                case 2:
                    break;
            }
        }
    }
}
