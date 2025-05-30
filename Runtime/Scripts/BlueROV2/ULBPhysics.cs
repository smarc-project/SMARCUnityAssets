using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using DefaultNamespace.LookUpTable;
using MathNet.Numerics.LinearAlgebra;


namespace DefaultNamespace
{
    public class ULBPhysics : MonoBehaviour
    {
        public ArticulationBody mainBody;
        
        private double m = 0; //mass kg
        private double I_x = 0;
        private double I_y = 0;
        private double I_z = 0;
        private double W = 0; //weight N
        private double B = 0; // bouyancy N
        double g = 9.82; // gravity m/s²
        double rho = 1000; // water density [kg/m^3]
        double nabla = 0.0000256; // volume
        double  x_b = 0; double y_b = 0; double z_b = 0;
        // Start is called before the first frame update
        void Start()
        {
            m = mainBody.mass; // mass 13.5
            I_x = mainBody.inertiaTensor.x;
            I_y = mainBody.inertiaTensor.z;
            I_z = mainBody.inertiaTensor.y; // y z switch. Unity to NED coordinates
            W = m * g; // weight
            B = rho*g*nabla; // The buoyancy in [N] given by OSBS
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Get world rotation
            var world_rot = mainBody.transform.rotation.eulerAngles; 
                
            //Get and convert state vector from global to local reference point
            var inverseTransformDirection = mainBody.transform.InverseTransformDirection(mainBody.linearVelocity); // Local frame vel
            var transformAngularVelocity = mainBody.transform.InverseTransformDirection(mainBody.angularVelocity); // Local frame angular vel (gives negative velocities)
                
            // Convert angles, angular velocities and velocities to OSBS coordinate system
            var phiThetaTau = FRD.ConvertAngularVelocityFromRUF(world_rot).ToDense();
            float phi = (float) (Mathf.Deg2Rad * phiThetaTau[0]); 
            float theta = (float) (Mathf.Deg2Rad* phiThetaTau[1]);
            
            // Restoring forces vector
            Vector<double> g_vec = Vector<double>.Build.DenseOfArray(new double[]
            {
                (W - B) * Mathf.Sin(theta),
                -(W - B) * Mathf.Cos(theta) * Mathf.Sin(phi),
                -(W - B) * Mathf.Cos(theta) * Mathf.Cos(phi),
                y_b * B * Mathf.Cos(theta) * Mathf.Cos(phi) - z_b * B * Mathf.Cos(theta) * Mathf.Sin(phi),
                -z_b * B * Mathf.Sin(theta) - x_b * B * Mathf.Cos(theta) * Mathf.Cos(phi),
                x_b * B * Mathf.Cos(theta) * Mathf.Sin(phi) + y_b * B * Mathf.Sin(theta)
            });
            
            var RestoringForce  = g_vec.SubVector(0, 3).ToVector3();
            var RestoringTorque = g_vec.SubVector(3, 3).ToVector3();
            RestoringForce = NED.ConvertToRUF(RestoringForce);
            RestoringTorque = FRD.ConvertAngularVelocityToRUF(RestoringTorque);
            mainBody.AddRelativeForce(-RestoringForce);
            mainBody.AddRelativeTorque(-RestoringTorque);
        }
    }
}