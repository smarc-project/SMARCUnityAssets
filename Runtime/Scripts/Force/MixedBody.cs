using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Water;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor.EditorTools;

namespace Force
{

    // Because Arti bodies and Rigid bodies dont share
    // an ancestor, even though they share like 99% of the
    // methods and semantics...public class MixedBody
    public class MixedBody
    {
        public ArticulationBody ab;
        public Rigidbody rb;

        public bool isValid => ab != null || rb != null;

        public MixedBody(ArticulationBody ab, Rigidbody rb)
        {
            this.ab = ab;
            this.rb = rb;
        }

        public GameObject gameObject
        {
            get {return ab ? ab.gameObject : rb.gameObject; }
        }

        public Transform transform
        {
            get {return ab ? ab.transform : rb.transform; }
        }

        public bool automaticCenterOfMass
        {
            get {return ab ? ab.automaticCenterOfMass : rb.automaticCenterOfMass; }
            set {
                if(ab != null) ab.automaticCenterOfMass = value;
                else rb.automaticCenterOfMass = value;
                }
        }

        public Vector3 centerOfMass
        {
            get {return ab ? ab.centerOfMass : rb.centerOfMass; }
            set {
                if(ab != null) ab.centerOfMass = value;
                else rb.centerOfMass = value;
            }
        }

        public bool useGravity
        {
            get {return ab ? ab.useGravity : rb.useGravity; }
            set {
                if(ab != null) ab.useGravity = value;
                else rb.useGravity = value;
            }
        }

        public float mass
        {
            get {return ab ? ab.mass : rb.mass; }
            set {
                if(ab != null) ab.mass = value;
                else rb.mass = value;
            }
        }

        public float drag
        {
            get {return ab ? ab.linearDamping : rb.drag; }
            set {
                if(ab != null) ab.linearDamping = value;
                else rb.drag = value;
            }
        }

        public float angularDrag
        {
            get {return ab ? ab.angularDamping : rb.angularDrag; }
            set {
                if(ab != null) ab.angularDamping = value;
                else rb.angularDrag = value;
            }
        }

        public Vector3 position
        {
            get {return ab ? ab.transform.position : rb.transform.position; }
        }

        public Quaternion rotation
        {
            get {return ab ? ab.transform.rotation : rb.transform.rotation; }
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
        {
            if(ab != null)
                ab.AddForceAtPosition(force, position, mode);
            else
                rb.AddForceAtPosition(force, position, mode);
        }

        public void ConnectToJoint(Joint j)
        {
            if(ab != null) j.connectedArticulationBody = ab;
            else j.connectedBody = rb;
        }

        public float GetTotalConnectedMass()
        {
            // too much hassle to follow through an arbitrary system
            // of rigidbodies and joints...
            if(ab == null) return rb.mass;

            // find the root of the articulation chain and sum up
            // the mass of all the bodies
            ArticulationBody root = ab;
            while (root.transform.parent != null)
            {
                var parentAB = root.transform.parent.GetComponent<ArticulationBody>();
                if(parentAB == null) break;
                root = parentAB;
                if(parentAB.isRoot) break;
            }

            float totalMass = 0f;
            var queue = new Queue<ArticulationBody>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                ArticulationBody current = queue.Dequeue();
                totalMass += current.mass;

                for (int i = 0; i < current.transform.childCount; i++)
                {
                    ArticulationBody child = current.transform.GetChild(i).GetComponent<ArticulationBody>();
                    if (child != null)
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return totalMass;

        }
    }
}
