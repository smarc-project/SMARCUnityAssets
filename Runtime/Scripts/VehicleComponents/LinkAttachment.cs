using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils = DefaultNamespace.Utils;

namespace VehicleComponents
{
    public class LinkAttachment : MonoBehaviour
    {
        [Header("Link attachment")] 
        [Tooltip("The name of the link the sensor should be attached to.")]
        public string linkName = "";

        [Tooltip("If true, will try on FixedUpdates to attach, if false attach only on Awake")]
        public bool retryUntilSuccess = true;

        [Tooltip("If ROS uses a different camera refenrece frame.")]
        public bool rotateForROSCamera = false;

        [Tooltip("Rotate the object with respect to the attached link after attaching.")]
        public float roll = 0f, pitch = 0f, yaw = 0f;

        protected GameObject attachedLink;
        protected ArticulationBody articulationBody;
        protected ArticulationBody parentArticulationBody;

        protected void Awake()
        {
            Attach();
        }

        protected void Attach()
        {
            // This object needs to dig down from the root
            // and find an object with the given name to become
            // a child under.
            attachedLink = Utils.FindDeepChildWithName(transform.root.gameObject, linkName);
            if (attachedLink == null)
            {
                Debug.Log($"Object with name [{linkName}] not found under parent [{transform.root.name}]. Disabling {this.name}.");
                enabled = false;
                return;
            }

            transform.SetPositionAndRotation
            (
                attachedLink.transform.position,
                attachedLink.transform.rotation
            );
            transform.Rotate(Vector3.up, yaw);
            transform.Rotate(Vector3.right, pitch);
            transform.Rotate(Vector3.forward, roll);

            // ...except if its a camera, ROS
            // defines it with Y forw, Z right, X up (mapped to unity)
            // instead of ZXY
            // so we gotta turn our ZXY camera to match the YZX frame
            if (rotateForROSCamera)
            {
                transform.Rotate(Vector3.up, 90);
                transform.Rotate(Vector3.right, -90);
                transform.Rotate(Vector3.forward, 180);
            }

            transform.SetParent(attachedLink.transform);

            TryGetComponent<ArticulationBody>(out articulationBody);
            attachedLink.TryGetComponent<ArticulationBody>(out parentArticulationBody);
            if (articulationBody == null) articulationBody = parentArticulationBody;
        }

        void FixedUpdate()
        {
            if(attachedLink == null && retryUntilSuccess) Attach();
        }

        void OnDrawGizmosSelected()
        {
            // Draw a semitransparent red cube at the transforms position
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));
        }

        void OnDrawGizmos()
        {
            // Draw a semitransparent green cube at the transforms position
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));
        }
    }
}