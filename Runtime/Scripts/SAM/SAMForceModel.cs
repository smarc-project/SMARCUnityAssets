using Force;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;

namespace DefaultNamespace
{
    public class SAMForceModel : MonoBehaviour, IForceModel, ISAMControl
    {
        private Rigidbody rigidBody;
        public double lcg { get; set; }
        public double vbs { get; set; }
        public SAMParameters parameters { get; set; }
        public float d_rudder { get; set; }
        public float d_aileron { get; set; }
        public double rpm1 { get; set; }
        public double rpm2 { get; set; }

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        public void SetRpm1(double rpm)
        {
            this.rpm1 = rpm;
        }

        public void SetRpm2(double rpm)
        {
            this.rpm2 = rpm;
        }

        public void SetRpm(double rpm1, double rpm2)
        {
            SetRpm1(rpm1);
            SetRpm2(rpm2);
        }

        public void SetRudderAngle(float dr)
        {
            d_rudder = dr;
        }

        public void SetElevatorAngle(float de)
        {
            d_aileron = de;
        }

        public void SetBatteryPack(double lcg)
        {
            this.lcg = lcg;
        }

        public void SetWaterPump(float vbs)
        {
            this.vbs = vbs;
        }


        private void FixedUpdate()
        {
            var mb = DenseMatrix.Build;
            var vb = DenseVector.Build;

            // According to harsha: x=forw, y=right, z=down
            // Unity is x=right y=up z=forward
            var x = transform.position.x;
            var y = transform.position.z;
            var z = -transform.position.y;

            var phi = transform.localEulerAngles.x;
            var theta = transform.localEulerAngles.z;
            var psi = -transform.localEulerAngles.y;

            var velocity = transform.InverseTransformDirection(rigidBody.linearVelocity);
            var u = velocity.x;
            var v = velocity.z;
            var w = -velocity.y;

            var angularVelocity = transform.InverseTransformDirection(rigidBody.angularVelocity);
            // originally pqr=zx-y
            var p = angularVelocity.x;
            var q = angularVelocity.z;
            var r = -angularVelocity.y;

            // Inertia tensor
            Matrix<double> I_o = DenseMatrix.OfArray(new double[,]
            {
                { rigidBody.inertiaTensor.x, 0, 0 },
                { 0, rigidBody.inertiaTensor.z, 0 },
                { 0, 0, -rigidBody.inertiaTensor.y }
            });

            var d_scale = 0.1f;
            var vbs_scale = 1;
            var lcg_scale = 1;

            var rpm1 = this.rpm1;
            var rpm2 = this.rpm2;
            var dElevator = d_aileron * d_scale;
            var dRudder = d_rudder * d_scale;
            var vbs = this.vbs * vbs_scale;
            var lcg = this.lcg * lcg_scale;

            // # Hydrodynamic coefficients. Damping
            // # Mostly pressure drag.
            var m = rigidBody.mass;
            var W = rigidBody.mass * 9.81;
            var B = W + vbs * 1.5;
            var Xuu = 5; // #1.0
            var Yvv = 20; // #100.0
            var Zww = 50; // #100.0
            var Kpp = 1; // #10.0
            var Mqq = 20; // #100.0
            var Nrr = 20; // #150.0

            // Center of gravity. Vehicle pointing down X axis.
            Vector<double> rg = Vector.Build.Dense(3, 0);
            rg[0] = lcg * 0.01;
            rigidBody.centerOfMass = rg.ToVector3();
            // Center of buoyancy position
            Vector<double> cb = Vector.Build.Dense(3, 0);
            // center of pressure position
            Vector<double> cp = Vector.Build.DenseOfArray(new[] { 0.1, 0, 0 });


            Vector<double> K_T = Vector.Build.DenseOfArray(new[] { 0.0175, 0.0175 });
            Vector<double> Q_T = Vector.Build.DenseOfArray(new[] { 0.01, 0.01 });


            Matrix<double> M = (mb.DenseIdentity(3) * m).Append(-m * mb.Skew(rg))
                .Stack(m * mb.Skew(rg).Append(I_o));

            var forces = mb.Diagonal(new double[] { Xuu * math.abs(u), Yvv * math.abs(v), Zww * math.abs(w), });
            var moments = mb.Diagonal(new double[] { Kpp * math.abs(p), Mqq * math.abs(q), Nrr * math.abs(r), });
            var coupling = mb.Skew(cp).Multiply(forces);

            Matrix<double> D = forces.Append(mb.DenseDiagonal(3, 0))
                .Stack((-coupling).Append(moments));

            var F_T = K_T.DotProduct(vb.DenseOfArray(new[] { rpm1, rpm2 }));
            var M_T = Q_T.DotProduct(vb.DenseOfArray(new[] { rpm1, rpm2 }));
            var M_Tx = Q_T.DotProduct(vb.DenseOfArray(new[] { rpm1, -rpm2 }));

            var control = vb.DenseOfArray(new[]
            {
                F_T * Mathf.Cos(dElevator) * Mathf.Cos(dRudder),
                F_T * Mathf.Sin(dElevator) * Mathf.Cos(dRudder),
                F_T * Mathf.Sin(dRudder),
                M_Tx * Mathf.Cos(dElevator) * Mathf.Cos(dRudder),
                M_T * Mathf.Sin(dRudder),
                M_T * Mathf.Sin(dElevator) * Mathf.Cos(dRudder),
            });

            var velocities = vb.DenseOfArray(new double[] { u, v, w, p, q, r, });
            var damping = -D.Multiply(velocities);

            // Map harsha's reference frame back to unity for forces
            var force_control_unity = Vector3.zero;
            var force_control = control.SubVector(0, 3).ToVector3();
            force_control_unity.x = force_control.z;
            force_control_unity.y = force_control.y * -1;
            force_control_unity.z = force_control.x;

            var torque_control = control.SubVector(3, 3).ToVector3();
            var torque_control_unity = Vector3.zero;
            torque_control_unity.x = torque_control.z * -1;
            torque_control_unity.y = torque_control.y;
            torque_control_unity.z = torque_control.x;

            var force_damping_unity = Vector3.zero;
            var force_damping = damping.SubVector(0, 3).ToVector3();
            force_damping_unity.x = force_damping.x;
            force_damping_unity.y = force_damping.z * -1;
            force_damping_unity.z = force_damping.y;

            var torque_damping_unity = Vector3.zero;
            var torque_damping = damping.SubVector(3, 3).ToVector3();
            torque_damping_unity.x = torque_damping.x;
            // torque_damping_unity.y = torque_damping.z * 1;
            torque_damping_unity.z = torque_damping.y;


            // Debug.Log("Control:  " + force_control + "  " + torque_control);
            // Debug.Log("Damping:  " + force_damping + "  " + torque_damping);

            rigidBody.AddRelativeForce(force_control_unity, ForceMode.Force);
            rigidBody.AddRelativeTorque(torque_control_unity, ForceMode.Force);

            TorqueDamping = torque_damping_unity;
            ForceDamping = force_damping_unity;

            rigidBody.AddRelativeForce(ForceDamping, ForceMode.Force);
            rigidBody.AddRelativeTorque(TorqueDamping, ForceMode.Force);
            // rigidBody.AddRelativeForce(force_damping, ForceMode.Force);
            // rigidBody.AddRelativeTorque(torque_damping, ForceMode.Force);
        }

        public Vector3 TorqueDamping { get; set; }
        public Vector3 ForceDamping { get; set; }

        public Vector3 GetTorqueDamping()
        {
            return TorqueDamping;
        }

        public Vector3 GetForceDamping()
        {
            return ForceDamping;
        }
    }
}