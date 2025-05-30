using System.Collections.Generic;
using UnityEngine;

using Force;


namespace GameUI
{
    public class ForceFieldVisualizer : MonoBehaviour
    {   
        [Header("Force Field Visualizer")]
        public GameObject ParticlePrefab;
        public int ParticleCount = 100;
        public float ParticleLifetime = 1f;
        public float ParticleSize = 0.1f;

        [Tooltip("If enabled, the particles will be recycled when they are out of all fields")]
        public bool RecycleWhenOut = true;
        [Tooltip("If enabled, particles will be spawned exactly inside the fields. This is expensive but might look nicer for some setups.")]
        public bool SpawnStrictlyInside = false;


        ForceFieldBase[] fields;
        Queue<ForceFieldParticle> particleQueue;

        void Awake()
        {
            if(ParticlePrefab == null)
            {
                Debug.LogError("ParticlePrefab is null!");
                enabled = false;
            }
        }


        void Start()
        {
            fields = FindObjectsByType<ForceFieldBase>(FindObjectsSortMode.None);
            // find fields where IncludeInVisualizer is true
            fields = System.Array.FindAll(fields, f => f.IncludeInVisualizer);
            
            particleQueue = new Queue<ForceFieldParticle>(ParticleCount);
            for(int i = 0; i < ParticleCount; i++)
            {
                var go = Instantiate(ParticlePrefab, transform);
                go.SetActive(false);
                var FFP = go.GetComponent<ForceFieldParticle>();
                FFP.Lifetime = Random.Range(0, ParticleLifetime);
                FFP.DeactivateWhenOut = RecycleWhenOut;
                FFP.Size = ParticleSize;
                particleQueue.Enqueue(FFP);
            }
        }

        void FixedUpdate()
        {
            if(particleQueue.Count == 0) return;
            if(fields.Length == 0) return;
            do
            {
                var particle = particleQueue.Dequeue();
                // active particle means its already spawned and moving around
                if(particle.gameObject.activeSelf) continue;
                var field = fields[Random.Range(0, fields.Length)];
                // maybe someone disabled/enabled a field at runtime?
                if(field.gameObject.activeSelf == false) continue;
                var position = field.GetRandomPointInside(strictlyInside: SpawnStrictlyInside);
                var color = field.onlyAboveWater? Color.red : field.onlyUnderwater? Color.blue : Color.green;
                particle.Spawn(position, color, particleQueue);

            } while(particleQueue.Count > 0);
        }
        
        
    }
}
