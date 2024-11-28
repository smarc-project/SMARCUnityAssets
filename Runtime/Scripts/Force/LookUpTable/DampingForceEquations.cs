using System;
using DefaultNamespace;
using DefaultNamespace.LookUpTable;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

namespace Force.LookUpTable
{
    public static class DampingForceEquations
    {
        //TODO: Not really constants
        public static double WaterDensity = 1023;
        public static double dVisc_DynamicViscosity = 0.0001002; // 1.002 e-3
        public static double C_SamLength = 1.2705;


        public static double Ar = 0.4754;
        public static double CKpp = 0.1;
        public static double CMqq = 40;
        public static double CNrr = 40;
        public static LookUpTables LookupTables;

        public static (Vector3 forces, Vector3 moments) CalculateDamping(Rigidbody rb, Transform samTransform)
        {
            var inverseTransformDirection = samTransform.InverseTransformDirection(rb.linearVelocity);
            var transformAngularVelocity = samTransform.InverseTransformDirection(rb.angularVelocity);
            var uvw_nm_nb = inverseTransformDirection.To<NED>().ToDense(); // Might need to revisit. Rel. velocity in point m block.
            var pqr_nm = FRD.ConvertAngularVelocityFromRUF(transformAngularVelocity).ToDense(); // FRD is same as NED for ANGLES ONLY

            var aoa_alpha_angleOfAttack = AngleOfAttack(uvw_nm_nb);
            var (forces, moments) = CalculateMomentsForces(uvw_nm_nb, pqr_nm, aoa_alpha_angleOfAttack);
            
            var forcesUnity = NED.ConvertToRUF(forces);
            var momentsUnity = FRD.ConvertAngularVelocityToRUF(moments);
            
            Debug.Log("Velocities: " + uvw_nm_nb.ToVector3() + " : " + pqr_nm.ToVector3() + "       Damping: " + forces + " : " + moments);
            // Debug.Log("RUF: " + inverseTransformDirection + " : " + transformAngularVelocity + "       NED: " + uvw_nm_nb.ToVector3() + " : " + pqr_nm.ToVector3() +                       "       BACK 2 RUF: " +NED.ConvertToRUF(uvw_nm_nb.ToVector3()) + " : " + FRD.ConvertAngularVelocityToRUF(pqr_nm.ToVector3()));

            return (forcesUnity, momentsUnity);
        }


        public static (Vector3 forces, Vector3 moments) CalculateMomentsForces(Vector<double> uvw_nm, Vector<double> pqr_nm, Vector<double> aoa)
        {
            var mb = DenseMatrix.Build;
            var vb = DenseVector.Build;

            var vInf = aoa[4];
            var Re_velocityReynolds = CalculateReynolds(vInf, WaterDensity, C_SamLength, dVisc_DynamicViscosity);
            // Should fetch based on Re, but for now use vinf
            var CX = FetchCoefficients_CX(vInf, aoa[2]);
            var CY = FetchCoefficients_CY(vInf, aoa[1]);
            var CZ = FetchCoefficients_CZ(vInf, aoa[0]);
            var XCp = FetchCoefficients_XCp(vInf, aoa[2]);
            var aoaTransversal = aoa[3];

            var u = uvw_nm[0];
            var v = uvw_nm[1];
            var w = uvw_nm[2];

            var p = pqr_nm[0];
            var q = pqr_nm[1];
            var r = pqr_nm[2];

            var ce = Vector.Build.DenseOfArray(new[] { XCp, 0, 0 });

            var translationalDampingCoefficients = Vector.Build.DenseOfArray(new[]
            {
                0.5 * WaterDensity * Ar * CX,
                0.5 * WaterDensity * Ar * CY,
                0.5 * WaterDensity * Ar * CZ
            });

            var rotationalDampingCoefficients = Vector.Build.DenseOfArray(new[]
            {
                -CKpp * p * Math.Abs(p),
                -CMqq * q * Math.Abs(q),
                -CNrr * r * Math.Abs(r)
            });

            var sth = mb.Diagonal(new[]
            {
                u * u, // Paper says Abs of |u| * u, etc
                v * v,
                w * w
            });

            var forces = -sth.Multiply(translationalDampingCoefficients);
            var moments = Vector3.Cross(ce.ToVector3(), forces.ToVector3());
            moments += rotationalDampingCoefficients.ToVector3();

            return (forces.ToVector3(), moments);
        }

        private static double CalculateReynolds(double v_Inf, double density, double c, double visc)
        {
            return v_Inf * density * c / visc;
        }

        public static Vector<double> AngleOfAttack(Vector<double> vr) // Relative Velocity, Center of Mass
        {
            var Vinf = vr.Norm(2);
            var a_angleOfAttack_alpha = Math.Atan2(vr[2], vr[0]);
            var b_beta_sideslip = Math.Asin(vr[1] / Vinf);
            var ae_effectiveAoA = Math.Acos(vr[0] / Vinf); // 
            var ta_transversalAoA = Math.Atan2(vr[2], vr[1]); //Sideways AoA
            return Vinf > 0.000001
                ? Vector.Build.DenseOfArray(new[] { a_angleOfAttack_alpha, b_beta_sideslip, ae_effectiveAoA, ta_transversalAoA, Vinf })
                : Vector.Build.DenseOfArray(new[] { 0, 0, 0, 0.0, 0.0 });
        }

        // Tables indexes are velocity / angle (degrees).
        // For example:
        // Velocity is from 0 to 10 with 0.1 step.
        // Angles are degrees, 360 with 1 degree step. 
        // Creates a table of 100 x 360 
        // So round to closest degree and velocity.
        // So round to closest decimal for velocity
        public static int TableVelocityIndex(double v_inf) //Should use raynolds instead, but we simplify for now.
        {
            return (int)Math.Round(Math.Abs(v_inf) * 10);
        }

        public static int TableDegreeIndex(double rad)
        {
            return (int)Math.Round(Mathf.Rad2Deg * rad) + 180;
        }

        public static double FetchCoefficients_CX(Double re, Double ae_effectiveAngleOfAttack)
        {
            return LookupTables.cx[TableVelocityIndex(re)][TableDegreeIndex(ae_effectiveAngleOfAttack)];
        }

        public static double FetchCoefficients_CY(Double re, Double b_beta)
        {
            return LookupTables.cy[TableVelocityIndex(re)][TableDegreeIndex(b_beta)];
        }

        public static double FetchCoefficients_CZ(Double re, Double a_alpha)
        {
            return LookupTables.cz[TableVelocityIndex(re)][TableDegreeIndex(a_alpha)];
        }

        public static double FetchCoefficients_XCp(Double re, Double ae_effectiveAngleOfAttack)
        {
            var fetchCoefficientsXCp = LookupTables.xcp[TableVelocityIndex(re)][TableDegreeIndex(ae_effectiveAngleOfAttack)];
            return (1 - fetchCoefficientsXCp) * C_SamLength;
        }
    }
}