using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using DefaultNamespace.LookUpTable;
using VehicleComponents.Actuators;
using Rope;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

/// <summary>
/// Tracking controller is implemented per "Geometric tracking control of a quadrotor UAV on SE(3)" 
/// Source: https://ieeexplore.ieee.org/document/5717652
/// 
/// </summary>
public class DroneLoadController: MonoBehaviour 
{
    [Header("Basics")]
    [Tooltip("Baselink of the drone")]
    public GameObject BaseLink;
    [Tooltip("Load's connection point to the rope")]
    public float ControlFrequency = 50f;
    [Tooltip("The maximum distance error between the load and the target position, kind of controls the aggressiveness of the maneuvers.")]
    public float DistanceErrorCap = 10f;

    [Header("Tracking")]
    [Tooltip("An object to follow")]
    public Transform TrackingTargetTF;

    [Header("Load")]
    [Tooltip("The rope object that this drone is expected to get connected, maybe. Will be used to check for attachment state and such.")]
    public Transform Rope; // TODO remove this requirement.
    [Tooltip("If true, instead of tracking the target object, drone will first track the buoy and when attached to it, make the LoadLinkTF track the target.")]
    public bool AttackTheBuoy = false;
    [Tooltip("The position of where the load is attached to the rope. rope_link on SAM")]
    public Transform LoadLinkTF; // The position of the AUV is taken at the base of the rope
    [Tooltip("Defines whether controller is in a load control state (when true), or in position tracking (when false)")]
    public bool LoadControl;


    [Header("Props")]
    public Transform PropFR;
    public Transform PropFL, PropBR, PropBL;



	Propeller[] propellers;
    float[] propellers_rpms;
    ArticulationBody base_link_ab;
    ArticulationBody load_link_ab;
    Matrix<double> R_sb_d_prev;
    Matrix<double> R_sb_c_prev;
    Vector<double> W_b_d_prev;
    Vector<double> W_b_c_prev;
    Vector<double> q_c_prev;
    Vector<double> q_c_dot_prev;
    int times1 = 0;
    int times2 = 0;


    // Quadrotor parameters
    double massQuadrotor;
    const double ROTOR_MOMENT_ARM = 0.315;
    Matrix<double> inertiaJ;
    const float TORQUE_COEFFICIENT = 8.004e-4f;

    // Map from propeller forces to Global Forces
    // Map when multiplied by [f1, f2, f3, f4].Transpose() createas the global forces [f, M_1, M_2, M_3].Traspose()
    static readonly Matrix<double> PROPELLOR_FORCE_TO_GLOBAL_MAP = DenseMatrix.OfArray(new double[,] 
    { 
        { 1, 1, 1, 1 },
        { 0, -ROTOR_MOMENT_ARM, 0, ROTOR_MOMENT_ARM },
        { ROTOR_MOMENT_ARM, 0, -ROTOR_MOMENT_ARM, 0 },
        { -TORQUE_COEFFICIENT, TORQUE_COEFFICIENT, -TORQUE_COEFFICIENT, TORQUE_COEFFICIENT }
    });

    // Load parameters
    double massLoad;
    double loadDistance;

    // Simulation parameters
    double g;
    static readonly Vector<double> E3 = DenseVector.OfArray(new double[] { 0, 0, 1 });
    float dt;
    float t;

    // Gains
    double kx;
    double kv;
    double kR;
    double kW;
    double kq;
    double kw;


	// Use this for initialization
	void Start()
    {
		propellers = new Propeller[4];
		propellers[0] = PropFL.GetComponent<Propeller>();
		propellers[1] = PropFR.GetComponent<Propeller>();
        propellers[2] = PropBR.GetComponent<Propeller>();
        propellers[3] = PropBL.GetComponent<Propeller>();

        base_link_ab = BaseLink.GetComponent<ArticulationBody>();

        // Creating identity matrices (3 x 3) for previous frame transforms
        R_sb_d_prev = DenseMatrix.CreateDiagonal(3, 3, 1.0);
        R_sb_c_prev = DenseMatrix.CreateDiagonal(3, 3, 1.0);

        // Example of how to instatiate a diagonal matrix
        // TODO: Delete later
        // double[] diagonal = {1,0,1};
        // Matrix<double> dummy = DenseMatrix.CreateDiagonal(3,3, index => diagonal[index]);
        
        W_b_d_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        W_b_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_dot_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });

		propellers_rpms = new float[] { 0, 0, 0, 0 };

        if(LoadLinkTF != null) load_link_ab = LoadLinkTF.GetComponent<ArticulationBody>();

        // Quadrotor parameters
        massQuadrotor = base_link_ab.mass;
        // Right hand rule from ROS, where in Unity the coordinate system is left hand rule, so need to flip 
        inertiaJ = DenseMatrix.OfArray(new double[,] { { base_link_ab.inertiaTensor.x, 0, 0 }, { 0, base_link_ab.inertiaTensor.z, 0 }, { 0, 0, base_link_ab.inertiaTensor.y } });

        massLoad = 0;
        // Use this load mass when load_link is on sam
        if(LoadLinkTF != null)
        {
            ArticulationBody[] sam_ab_list = LoadLinkTF.root.gameObject.GetComponentsInChildren<ArticulationBody>();
            foreach (ArticulationBody sam_ab in sam_ab_list)
            {
                massLoad += sam_ab.mass;
            }
        }
        // mL = 15;
        // Rope length l is calculated dynamically

        // Simulation parameters
        g = Physics.gravity.magnitude;
        dt = 1f/ControlFrequency;

        // TODO: get this working by smoothing out network effects
        // InvokeRepeating("ComputeRPMs", Time.fixedDeltaTime, dt);
	}

	// Update is called once per frame
	void FixedUpdate()
    {
		ComputeRPMs();
        ApplyRPMs();
	}

    (double, Vector<double>) SuspendedLoadControl()
    {
        double f;
        Vector<double> M;

        // Gains
        kx = 16*massQuadrotor;
        kv = 5.6*massQuadrotor;
        kR = 10;
        kW = 0.5;
        kq = 2;
        kw = 0.5;

        // Quadrotor states
        Vector<double> xQ_s = BaseLink.transform.position.To<ENU>().ToDense();
        Vector<double> vQ_s = base_link_ab.velocity.To<ENU>().ToDense();
        Matrix<double> R_sb = DenseMatrix.OfArray(new double[,] { { BaseLink.transform.right.x, BaseLink.transform.forward.x, BaseLink.transform.up.x },
                                                                { BaseLink.transform.right.z, BaseLink.transform.forward.z, BaseLink.transform.up.z },
                                                                { BaseLink.transform.right.y, BaseLink.transform.forward.y, BaseLink.transform.up.y } });
        Vector<double> W_b = -1f*(BaseLink.transform.InverseTransformDirection(base_link_ab.angularVelocity)).To<ENU>().ToDense();

        // Load states
        Vector<double> xL_s = LoadLinkTF.position.To<ENU>().ToDense();
        Vector<double> vL_s = load_link_ab.velocity.To<ENU>().ToDense();
        loadDistance = (xL_s - xQ_s).Norm(2);
        Vector<double> q = (xL_s - xQ_s)/loadDistance;
        Vector<double> q_dot = (vL_s - vQ_s)/loadDistance;

        // Desired states
        Vector<double> xL_s_d;//Math.Pow(0.5*t-5, 2) });
        Vector<double> vL_s_d;//0.5*t-5 });
        Vector<double> aL_s_d;//0.5 });

        xL_s_d = TrackingTargetTF.position.To<ENU>().ToDense();
        vL_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        aL_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });


        Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2)/2, Math.Sqrt(2)/2, 0 });

        // Load position controller
        Vector<double> ex = (xL_s - xL_s_d)*Math.Min(DistanceErrorCap/(xL_s - xL_s_d).Norm(2), 1);
        Vector<double> ev = vL_s - vL_s_d;

        Vector<double> A = -kx*ex - kv*ev + (massQuadrotor+massLoad)*(aL_s_d + g*E3) + massQuadrotor*loadDistance*(q_dot*q_dot)*q;
        Vector<double> q_c = -A/A.Norm(2);
        Vector<double> q_c_dot = DenseVector.OfArray(new double[] { 0, 0, 0 });//(q_c - q_c_prev)/dt;
        Vector<double> q_c_ddot = DenseVector.OfArray(new double[] { 0, 0, 0 });//(q_c_dot - q_c_dot_prev)/dt;
        Vector<double> F_n = (A*q)*q;
        Debug.DrawRay(ToUnityVector(xQ_s), ToUnityVector(q_c), Color.magenta);

        // Load attitude controller
        Vector<double> eq = _HatMap(q)*_HatMap(q)*q_c;
        Vector<double> eq_dot = q_dot - _Cross(_Cross(q_c, q_c_dot), q);

        Vector<double> F_pd = -kq*eq - kw*eq_dot;
        Vector<double> F_ff = massQuadrotor*loadDistance*(q*_Cross(q_c, q_c_dot))*_Cross(q, q_dot) + massQuadrotor*loadDistance*_Cross(_Cross(q_c, q_c_ddot), q);
        Vector<double> F_for_f = F_n - F_pd - F_ff;

        F_n = -(q_c*q)*q;
        Vector<double> F_for_M = F_n - F_pd - F_ff;

        // Quadrotor attitude controller
        Vector<double> b3c = F_for_M/F_for_M.Norm(2);
        Vector<double> b1c = -_Cross(b3c, _Cross(b3c, b1d))/_Cross(b3c, b1d).Norm(2);
        Vector<double> b2c = _Cross(b3c, b1c);
        Matrix<double> R_sb_c = DenseMatrix.OfArray(new double[,] { { b1c[0], b2c[0], b3c[0] },
                                                                    { b1c[1], b2c[1], b3c[1] },
                                                                    { b1c[2], b2c[2], b3c[2] } });

        Vector<double> W_b_c = _VeeMap(_Logm3(R_sb_c_prev.Transpose()*R_sb_c)/dt);
        Vector<double> W_b_c_dot = (W_b_c - W_b_c_prev)/dt;

        Vector<double> eR = 0.5*_VeeMap(R_sb_c.Transpose()*R_sb - R_sb.Transpose()*R_sb_c);
        Vector<double> eW = W_b - R_sb.Transpose()*R_sb_c*W_b_c;

        f = F_for_f*(R_sb*E3);
        M = -kR*eR - kW*eW + _Cross(W_b, inertiaJ*W_b) - inertiaJ*(_HatMap(W_b)*R_sb.Transpose()*R_sb_c*W_b_c - R_sb.Transpose()*R_sb_c*W_b_c_dot);

        // Transform M to NED frame (from ENU) for the propeller forces mapping
        Matrix<double> R_ws = DenseMatrix.OfArray(new double[,] { { 0, 1, 0 },
                                                                    { 1, 0, 0 },
                                                                    { 0, 0, -1 } });
        M = R_ws*M;

        // Save previous values
        R_sb_c_prev = R_sb_c;
        W_b_c_prev = W_b_c;
        q_c_prev = q_c;
        q_c_dot_prev = q_c_dot;

        if (times1 < 2)
        {
            times1++;
            f = 0;
            M = DenseVector.OfArray(new double[] { 0, 0, 0 });
        }

        return (f, M);
    }

    (double, Vector<double>) TrackingControl()
    {
        double f;
        Vector<double> M;

        // Gains
        kx = 16*massQuadrotor;
        kv = 5.6*massQuadrotor;
        kR = 8.81;
        kW = 2.54;

        // Quadrotor states
        Vector<double> x_s = BaseLink.transform.position.To<NED>().ToDense();
        Vector<double> v_s = base_link_ab.velocity.To<NED>().ToDense();
        Matrix<double> R_wa = DenseMatrix.OfArray(new double[,] { { BaseLink.transform.right.x, BaseLink.transform.forward.x, BaseLink.transform.up.x },
                                                                { BaseLink.transform.right.z, BaseLink.transform.forward.z, BaseLink.transform.up.z },
                                                                { BaseLink.transform.right.y, BaseLink.transform.forward.y, BaseLink.transform.up.y } });
        Vector<double> W_b = FRD.ConvertAngularVelocityFromRUF(BaseLink.transform.InverseTransformDirection(base_link_ab.angularVelocity)).ToDense();

        // Transformations
        Matrix<double> R_ws = DenseMatrix.OfArray(new double[,] { { 0, 1, 0 },
                                                                { 1, 0, 0 },
                                                                { 0, 0, -1 } });
        Matrix<double> R_ab = R_ws;
        Matrix<double> R_sw = R_ws.Transpose();
        Matrix<double> R_sb = R_sw*R_wa*R_ab;
        Matrix<double> R_sa = R_sw*R_wa;
        Matrix<double> R_bs = R_sb.Transpose();
        Matrix<double> R_bw = R_bs*R_sw;

        // Desired states
        Vector<double> x_s_d;
        Vector<double> v_s_d;
        Vector<double> a_s_d;

        x_s_d = TrackingTargetTF.position.To<NED>().ToDense();
        v_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });

        if(AttackTheBuoy && Rope != null)
        {
            Vector<double> buoy_w = R_ws*Rope.GetChild(Rope.childCount-1).position.To<NED>().ToDense();
            x_s_d = R_sw*DenseVector.OfArray(new double[] { buoy_w[0], buoy_w[1], Math.Pow(t-4, 2)/16 + buoy_w[2] + 0.16 });
            v_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, (t-4)/8 });
            a_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 1/8 });
        }

        Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2)/2, Math.Sqrt(2)/2, 0 });

        // Control
        Vector<double> ex = (x_s - x_s_d)*Math.Min(DistanceErrorCap/(x_s - x_s_d).Norm(2), 1);
        Vector<double> ev = v_s - v_s_d;

        Vector<double> pid = -kx*ex - kv*ev - massQuadrotor*g*E3 + massQuadrotor*a_s_d;
        Vector<double> b3d = -pid/pid.Norm(2);
        Vector<double> b2d = _Cross(b3d, b1d)/_Cross(b3d, b1d).Norm(2);
        Vector<double> b1d_temp = _Cross(b2d, b3d);
        Matrix<double> R_sb_d = DenseMatrix.OfArray(new double[,] { { b1d_temp[0], b2d[0], b3d[0] },
                                                                    { b1d_temp[1], b2d[1], b3d[1] },
                                                                    { b1d_temp[2], b2d[2], b3d[2] } });

        Vector<double> W_b_d = _VeeMap(_Logm3(R_sb_d_prev.Transpose()*R_sb_d)/dt);
        Vector<double> W_b_d_dot = (W_b_d - W_b_d_prev)/dt;

        Vector<double> eR = 0.5*_VeeMap(R_sb_d.Transpose()*R_sb - R_sb.Transpose()*R_sb_d);
        Vector<double> eW = W_b - R_sb.Transpose()*R_sb_d*W_b_d;

        f = -pid*(R_sb*E3);
        M = -kR*eR - kW*eW + _Cross(W_b, inertiaJ*W_b) - inertiaJ*(_HatMap(W_b)*R_sb.Transpose()*R_sb_d*W_b_d - R_sb.Transpose()*R_sb_d*W_b_d_dot);

        R_sb_d_prev = R_sb_d;
        W_b_d_prev = W_b_d;

        if (times2 < 2 || M.Norm(2) > 100)
        {
            times2++;
            f = 0;
            M = DenseVector.OfArray(new double[] { 0, 0, 0 });
        }
        // Debug.Log($"R_sb: {R_sb}, R_sb_d: {R_sb_d} R_sb_d_prev: {R_sb_d_prev}");
        return (f, M);
    }

	void ComputeRPMs()
    {
        t = Time.time;

        double f;
        Vector<double> M;

        // If rope has been replaced (tension is high enough) use suspended load controller
        // If we have not hooked the rope yet, use normal tracking controller
        if (Rope != null && Rope.childCount == 2) (f, M) = SuspendedLoadControl();
        else (f, M) = TrackingControl();

        Vector<double> globalForces = _StackForceMomentVector(f, M);
        Vector<double> F = PROPELLOR_FORCE_TO_GLOBAL_MAP.Inverse() * globalForces;

        // Debug.Log($"f: {f}, M: {M}");

        // Set propeller rpms
        for (int i = 0; i < propellers.Length; i++)
            propellers_rpms[i] = (float)F[i]/propellers[i].RPMToForceMultiplier;
	}

	void ApplyRPMs()
    {
        // TODO: try clamping rpms to zero
		for (int i = 0; i < propellers.Length; i++)
            propellers[i].SetRpm(propellers_rpms[i]);
	}

    /// <summary>
    /// Cross product operation for R^3 vectors
    /// </summary>
    static Vector<double> _Cross(Vector<double> a, Vector<double> b)
    {
        // Calculate each component of the cross product
        double c1 = a[1] * b[2] - a[2] * b[1];
        double c2 = a[2] * b[0] - a[0] * b[2];
        double c3 = a[0] * b[1] - a[1] * b[0];

        // Create a new vector for the result
        return DenseVector.OfArray(new double[] { c1, c2, c3 });
    }

    /// <summary>
    ///  Constructs skew symmetric matrix from vector. Also known as the hat map.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private static Matrix<double> _HatMap(Vector<double> v)
    {
        return DenseMatrix.OfArray(new double[,] { { 0, -v[2], v[1] },
                                                   { v[2], 0, -v[0] },
                                                   { -v[1], v[0], 0 } });
    }

    /// <summary>
    ///  Constructs vector from skew symmetric matrix. Also known as the vee map.
    /// </summary>
    /// <param name="S"></param>
    /// <returns></returns>
    private static Vector<double> _VeeMap(Matrix<double> S)
    {
        return DenseVector.OfArray(new double[] { S[2, 1], S[0, 2], S[1, 0] });
    }

    /// <summary>
    /// Stacks global force and moment vector as: [f moment[0] moment[1] moment[2]].Transpose() where 0-2 refer to axis index
    /// 
    /// <para>
    /// f, referes to global force along axis of propellors hence it is only a scaler
    /// </para>
    /// </summary>
    /// <param name="f"></param>
    /// <param name="moments"></param>
    /// <returns></returns>
    private static Vector<double> _StackForceMomentVector(double f, Vector<double> moments){
        return DenseVector.OfArray(new double[] { f, moments[0], moments[1], moments[2] });
    }

    static Matrix<double> _Logm3(Matrix<double> R)
    {
		double acosinput = (R[0, 0] + R[1, 1] + R[2, 2] - 1) / 2.0;
		Matrix<double> m_ret = DenseMatrix.OfArray(new double[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } });
		if (acosinput >= 1)
			return m_ret;
		else if (acosinput <= -1) {
			Vector<double> omg;
			if (!(Math.Abs(1 + R[2, 2]) < 1e-6f))
				omg = (1.0 / Math.Sqrt(2 * (1 + R[2, 2])))*DenseVector.OfArray(new double[] { R[0, 2], R[1, 2], 1 + R[2, 2] });
			else if (!(Math.Abs(1 + R[1, 1]) < 1e-6f))
				omg = (1.0 / Math.Sqrt(2 * (1 + R[1, 1])))*DenseVector.OfArray(new double[] { R[0, 1], 1 + R[1, 1], R[2, 1] });
			else
				omg = (1.0 / Math.Sqrt(2 * (1 + R[0, 0])))*DenseVector.OfArray(new double[] { 1 + R[0, 0], R[1, 0], R[2, 0] });
			m_ret = _HatMap(Math.PI * omg);
			return m_ret;
		}
		else {
			double theta = Math.Acos(acosinput);
			m_ret = theta / 2.0 / Math.Sin(theta)*(R - R.Transpose());
			return m_ret;
		}
	}

    private static Vector3 ToUnityVector(Vector<double> v)
    {
        return new Vector3((float)v[0], (float)v[2], (float)v[1]);
    }
}
