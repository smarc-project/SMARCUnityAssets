using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Force;

namespace GameUI
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TrailRenderer))]
    public class ForceFieldParticle : MonoBehaviour
    {
        [Header("Force Field Particle")]
        public float Lifetime = 1f;
        public float Size = 0.1f;
        public bool DeactivateWhenOut = true;

        float remainingLifetime = 0f;
        float remainingCooldown = 0f;

        ForcePoint FP;
        Rigidbody RB;
        TrailRenderer TR;
        SimpleGizmo FPgizmo;
        Queue<ForceFieldParticle> queue;

        void Awake()
        {
            var fp = transform.Find("ForcePoint");
            if(fp != null)
            {
                FP = fp.GetComponent<ForcePoint>();
            }
            else
            {
                Debug.LogWarning("ForceFieldParticle does not have a ForcePoint child!");
                gameObject.SetActive(false);
            }
            
            RB = GetComponent<Rigidbody>();
            TR = GetComponent<TrailRenderer>();
            FPgizmo = FP.GetComponent<SimpleGizmo>();
            RB.isKinematic = true;
        }
        


        public void Spawn(Vector3 position, Color color, Queue<ForceFieldParticle> queue)
        {
            RB.isKinematic = true;
            transform.position = position;
            remainingLifetime = Lifetime;      
            gameObject.SetActive(true);
            TR.time = Lifetime;
            TR.startWidth = Size;
            TR.endWidth = 0f;
            TR.startColor = color;
            TR.endColor = new Color(color.r, color.g, color.b, 0f);
            FPgizmo.radius = Size;
            FP.enabled = true;
            this.queue = queue;
        }


        void FixedUpdate()
        {
            if(queue == null) return;
            if(remainingLifetime > 0) remainingLifetime -= Time.fixedDeltaTime;
            TR.startWidth = Size * (remainingLifetime / Lifetime);
            bool dead = remainingLifetime <= 0 || (DeactivateWhenOut && RB.GetAccumulatedForce() == Vector3.zero);
            RB.isKinematic = dead;
            FP.enabled = !dead;
            if(dead)
            {
                if(remainingCooldown == 0) remainingCooldown = Lifetime;
                remainingCooldown -= Time.fixedDeltaTime;
                if(remainingCooldown <= 0)
                {
                    TR.Clear();
                    gameObject.SetActive(false);
                    queue.Enqueue(this);
                    remainingCooldown = 0f;
                    return;
                }
            }
           
        }


    }
}