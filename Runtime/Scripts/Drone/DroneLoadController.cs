using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using DefaultNamespace.LookUpTable;
using VehicleComponents.Actuators;
using Rope;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class DroneLoadController: MonoBehaviour 
{
    [Header("Basics")]
    [Tooltip("Baselink of the drone")]
    public GameObject BaseLink;
    // [Tooltip("Load's connection point to the rope")]
    // public float ControlFrequency = 50f;
    // [Tooltip("The maximum distance error between the load and the target position, kind of controls the aggressiveness of the maneuvers.")]
    // public float DistanceErrorCap = 10f;
    // private Vector<double> startingPosition = null;
    public float MaxVelocityWithTrackingTarget = 1f;
    // public float MaxAccelerationWithTrackingTarget = 1f;
    public float DecelerationDistance = 0.1f;

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
    double mQ;
    double d;
    Matrix<double> J;
    float c_tau_f;
    Matrix<double> T;
    Matrix<double> T_inv;
    Matrix<double> Q;
    const int NUM_PROPS = 4;

    // Load parameters
    double mL;
    double l;

    // Simulation parameters
    double g;
    Vector<double> e3;
    float dt;

    // Gains
    double kx;
    double kv;
    double kR;
    double kW;
    double kq;
    double kw;

    double[] coeffsX = new double[6];
    double[] coeffsY = new double[6];
    double[] coeffsZ = new double[6];
    int min_snap_flag;
    double catching_time; 

    // Logging
    string filePath = Application.dataPath + "/../../SMARCUnityAssets/Logs/log.csv";
    TextWriter tw;

	// Use this for initialization
	void Start() 
    {
		propellers = new Propeller[4];
		propellers[0] = PropFL.GetComponent<Propeller>();
		propellers[1] = PropFR.GetComponent<Propeller>();
        propellers[2] = PropBR.GetComponent<Propeller>();
        propellers[3] = PropBL.GetComponent<Propeller>();

        base_link_ab = BaseLink.GetComponent<ArticulationBody>();

        R_sb_d_prev = DenseMatrix.OfArray(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        R_sb_c_prev = DenseMatrix.OfArray(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        W_b_d_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        W_b_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_dot_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });

		propellers_rpms = new float[] { 0, 0, 0, 0 }; // 0.0666 g of thrust per rpm, maxrpm is 10656

        if(LoadLinkTF != null) load_link_ab = LoadLinkTF.GetComponent<ArticulationBody>();
        
        // Quadrotor parameters
        mQ = base_link_ab.mass;
        d = 0.315;
        J = DenseMatrix.OfArray(new double[,] { { base_link_ab.inertiaTensor.x, 0, 0 }, { 0, base_link_ab.inertiaTensor.z, 0 }, { 0, 0, base_link_ab.inertiaTensor.y } });
        c_tau_f = 0.08f;

        mL = 0;
        // Use this load mass when load_link is on sam
        if(LoadLinkTF != null)
        {
            ArticulationBody[] sam_ab_list = LoadLinkTF.root.gameObject.GetComponentsInChildren<ArticulationBody>();
            foreach (ArticulationBody sam_ab in sam_ab_list) 
            {
                mL += sam_ab.mass;
            }
        }
        // mL = 15;
        // Rope length l is calculated dynamically

        // Simulation parameters
        g = Physics.gravity.magnitude;
        e3 = DenseVector.OfArray(new double[] { 0, 0, 1 });
        dt = Time.fixedDeltaTime;//1f/ControlFrequency;

        T = DenseMatrix.OfArray(new double[,] { { 1, 1, 1, 1 },
                                                    { d, 0, -d, 0 },
                                                    { 0, -d, 0, d },
                                                    { c_tau_f, -c_tau_f, c_tau_f, -c_tau_f } });
        T_inv = T.Inverse();
        Q = T.Transpose()*T;

        //  One dimensional 
        /*
        // Define the start and end conditions for the trajectory
        double startPos = 0.0, startVel = 0.0, startAcc = 0.0;
        double endPos = 10.0, endVel = 0.0, endAcc = 0.0;
        double T = 5.0; // Total time for trajectory

        // Calculate the minimum snap trajectory coefficients
        double[] coeffs = MinimumSnapTrajectory.MinimumSnapCoefficients(startPos, startVel, startAcc, endPos, endVel, endAcc, T);

        // Print the calculated polynomial coefficients
        Debug.Log("Minimum Snap Trajectory Coefficients:");
        for (int i = 0; i < coeffs.Length; i++)
        {
            Debug.Log($"Coefficient a{i}: {coeffs[i]}");
        }

        // Evaluate the position at different time points and print the results
        Debug.Log("\nEvaluating Trajectory at various time points:");
        for (double t = 0; t <= T; t += 0.5)
        {
            double positionAtT = MinimumSnapTrajectory.EvaluatePolynomial(coeffs, t);
            Debug.Log($"Position at time t={t}: {positionAtT}");
        }
        */

        //  Three dimensional 
        // Define start and end positions, velocities, and accelerations for x, y, z

        //
        min_snap_flag = 0;
        catching_time = 0; 
        /*
        Vector<double> x_s = BaseLink.transform.position.To<NED>().ToDense();
        Vector3 startPos = new Vector3((float)x_s[0], (float)x_s[1], (float)x_s[2]);
        // Vector3 startPos = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 startVel = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 startAcc = new Vector3(0.0f, 0.0f, 0.0f);


        // Transformations
        Matrix<double> R_ws = DenseMatrix.OfArray(new double[,] { { 0, 1, 0 },
                                                                { 1, 0, 0 },
                                                                { 0, 0, -1 } });

        Vector<double> buoy_w = R_ws*Rope.GetChild(Rope.childCount-1).position.To<NED>().ToDense();
        Vector3 endPos = new Vector3((float)buoy_w[0], (float)buoy_w[1], (float)buoy_w[2]);
        //Vector3 endPos = new Vector3(10.0f, 5.0f, 3.0f);
        Vector3 endVel = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 endAcc = new Vector3(0.0f, 0.0f, 0.0f);

        double T = 5.0; // Total time for trajectory

        // Calculate minimum snap trajectory coefficients for each axis (x, y, z)
        double[] coeffsX = MinimumSnapTrajectory.MinimumSnapCoefficients(startPos.x, startVel.x, startAcc.x, endPos.x, endVel.x, endAcc.x, T);
        double[] coeffsY = MinimumSnapTrajectory.MinimumSnapCoefficients(startPos.y, startVel.y, startAcc.y, endPos.y, endVel.y, endAcc.y, T);
        double[] coeffsZ = MinimumSnapTrajectory.MinimumSnapCoefficients(startPos.z, startVel.z, startAcc.z, endPos.z, endVel.z, endAcc.z, T);

        // Print the calculated polynomial coefficients for each axis
        Debug.Log("Minimum Snap Trajectory Coefficients (X, Y, Z):");

        // Print coefficients for X
        Debug.Log("X axis:");
        for (int i = 0; i < coeffsX.Length; i++)
        {
            Debug.Log($"Coefficient a{i}: {coeffsX[i]}");
        }

        // Print coefficients for Y
        Debug.Log("Y axis:");
        for (int i = 0; i < coeffsY.Length; i++)
        {
            Debug.Log($"Coefficient a{i}: {coeffsY[i]}");
        }

        // Print coefficients for Z
        Debug.Log("Z axis:");
        for (int i = 0; i < coeffsZ.Length; i++)
        {
            Debug.Log($"Coefficient a{i}: {coeffsZ[i]}");
        }

        // Evaluate the position at different time points for all axes
        Debug.Log("\nEvaluating Trajectory at various time points:");
        for (double t = 0; t <= T; t += 0.5)
        {
            double posX = MinimumSnapTrajectory.EvaluatePolynomial(coeffsX, t);
            double posY = MinimumSnapTrajectory.EvaluatePolynomial(coeffsY, t);
            double posZ = MinimumSnapTrajectory.EvaluatePolynomial(coeffsZ, t);

            // Evaluate velocity (first derivative)
            double velX = MinimumSnapTrajectory.EvaluatePolynomialDerivative(coeffsX, t);
            double velY = MinimumSnapTrajectory.EvaluatePolynomialDerivative(coeffsY, t);
            double velZ = MinimumSnapTrajectory.EvaluatePolynomialDerivative(coeffsZ, t);

            // Evaluate acceleration (second derivative)
            double accX = MinimumSnapTrajectory.EvaluatePolynomialSecondDerivative(coeffsX, t);
            double accY = MinimumSnapTrajectory.EvaluatePolynomialSecondDerivative(coeffsY, t);
            double accZ = MinimumSnapTrajectory.EvaluatePolynomialSecondDerivative(coeffsZ, t);

            // Print the 3D position, velocity, and acceleration at time t
            Debug.Log($"Time t={t}: Position=({posX}, {posY}, {posZ}), Velocity=({velX}, {velY}, {velZ}), Acceleration=({accX}, {accY}, {accZ})");
            // Print the 3D position at time t
            // Debug.Log($"Position at time t={t}: ({posX}, {posY}, {posZ})");
        }
        
        */

        tw = new StreamWriter(filePath, false);
        tw.WriteLine("t,x_s1,x_s2,x_s3,x_s_d1,x_s_d2,x_s_d3");
        tw.Close();

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
        kx = 16*mQ;
        kv = 5.6*mQ;
        kR = 8.81;
        kW = 0.5;
        kq = 2;
        kw = 0.5;
        
        // Quadrotor states
        Vector<double> xQ_s = BaseLink.transform.position.To<ENU>().ToDense();
        Vector<double> vQ_s = base_link_ab.linearVelocity.To<ENU>().ToDense();
        Matrix<double> R_sb = DenseMatrix.OfArray(new double[,] { { BaseLink.transform.right.x, BaseLink.transform.forward.x, BaseLink.transform.up.x },
                                                                { BaseLink.transform.right.z, BaseLink.transform.forward.z, BaseLink.transform.up.z },
                                                                { BaseLink.transform.right.y, BaseLink.transform.forward.y, BaseLink.transform.up.y } });
        Vector<double> W_b = -1f*(BaseLink.transform.InverseTransformDirection(base_link_ab.angularVelocity)).To<ENU>().ToDense();

        // Load states
        Vector<double> xL_s = LoadLinkTF.position.To<ENU>().ToDense();
        Vector<double> vL_s = load_link_ab.linearVelocity.To<ENU>().ToDense();
        l = (xL_s - xQ_s).Norm(2);
        Vector<double> q = (xL_s - xQ_s)/l;
        Vector<double> q_dot = (vL_s - vQ_s)/l;

        // Desired states
        Vector<double> xL_s_d;//Math.Pow(0.5*t-5, 2) });
        Vector<double> vL_s_d;//0.5*t-5 });
        Vector<double> aL_s_d;//0.5 });
        (xL_s_d, vL_s_d, aL_s_d) = TrackingTargetTrajectory(TrackingTargetTF.position.To<ENU>().ToDense(), xL_s, vL_s);
        
        Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2)/2, -Math.Sqrt(2)/2, 0 });

        // Load position controller
        Vector<double> ex = (xL_s - xL_s_d);//*Math.Min(DistanceErrorCap/(xL_s - xL_s_d).Norm(2), 1);
        Vector<double> ev = vL_s - vL_s_d;

        Vector<double> A = -kx*ex - kv*ev + (mQ+mL)*(aL_s_d + g*e3) + mQ*l*(q_dot*q_dot)*q;
        Vector<double> q_c = -A/A.Norm(2);
        Vector<double> q_c_dot = DenseVector.OfArray(new double[] { 0, 0, 0 });//(q_c - q_c_prev)/dt;
        Vector<double> q_c_ddot = DenseVector.OfArray(new double[] { 0, 0, 0 });//(q_c_dot - q_c_dot_prev)/dt;
        Vector<double> F_n = (A*q)*q;
        Debug.DrawRay(ToUnity(xQ_s), ToUnity(q_c), Color.magenta);

        // Load attitude controller
        Vector<double> eq = _Hat(q)*_Hat(q)*q_c;
        Vector<double> eq_dot = q_dot - _Cross(_Cross(q_c, q_c_dot), q);
        
        Vector<double> F_pd = -kq*eq - kw*eq_dot;
        Vector<double> F_ff = mQ*l*(q*_Cross(q_c, q_c_dot))*_Cross(q, q_dot) + mQ*l*_Cross(_Cross(q_c, q_c_ddot), q);
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

        Vector<double> W_b_c = _Vee(_Logm3(R_sb_c_prev.Transpose()*R_sb_c)/dt);
        Vector<double> W_b_c_dot = (W_b_c - W_b_c_prev)/dt;

        Vector<double> eR = 0.5*_Vee(R_sb_c.Transpose()*R_sb - R_sb.Transpose()*R_sb_c);
        Vector<double> eW = W_b - R_sb.Transpose()*R_sb_c*W_b_c;

        f = F_for_f*(R_sb*e3);
        M = -kR*eR - kW*eW + _Cross(W_b, J*W_b) - J*(_Hat(W_b)*R_sb.Transpose()*R_sb_c*W_b_c - R_sb.Transpose()*R_sb_c*W_b_c_dot);
        
        // Save previous values
        R_sb_c_prev = R_sb_c;
        W_b_c_prev = W_b_c;
        q_c_prev = q_c;
        q_c_dot_prev = q_c_dot;

        if (times1 < 2 || M.Norm(2) > 100)
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
        kx = 16*mQ;
        kv = 5.6*mQ;
        kR = 8.81;
        kW = 2.54;
        
        // Quadrotor states
        Vector<double> x_s = BaseLink.transform.position.To<ENU>().ToDense();
        Vector<double> v_s = base_link_ab.velocity.To<ENU>().ToDense();
        Matrix<double> R_sb = DenseMatrix.OfArray(new double[,] { { BaseLink.transform.right.x, BaseLink.transform.forward.x, BaseLink.transform.up.x },
                                                                { BaseLink.transform.right.z, BaseLink.transform.forward.z, BaseLink.transform.up.z },
                                                                { BaseLink.transform.right.y, BaseLink.transform.forward.y, BaseLink.transform.up.y } });
        Vector<double> W_b = -1f*BaseLink.transform.InverseTransformDirection(base_link_ab.angularVelocity).To<ENU>().ToDense();

        // Desired states
        Vector<double> x_s_d;
        Vector<double> v_s_d;
        Vector<double> a_s_d;
        (x_s_d, v_s_d, a_s_d) = TrackingTargetTrajectory(TrackingTargetTF.position.To<ENU>().ToDense(), x_s, v_s);

        // Minum snap trajectory parameters 
        double T = 5.0; // Total time for trajectory
        


        if(AttackTheBuoy && Rope != null)
        {
            if(min_snap_flag == 0)
            {
                min_snap_flag = 1;
            }


            if(min_snap_flag == 1)
            {
                Vector3 startPos = new Vector3((float)x_s[0], (float)x_s[1], (float)x_s[2]);
                // Vector3 startPos = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 startVel = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 startAcc = new Vector3(0.0f, 0.0f, 0.0f);

                Vector3<ENU> endPosENU = Rope.GetChild(Rope.childCount-1).position.To<ENU>();
                Vector3 endPos = new Vector3(endPosENU.x, endPosENU.y, endPosENU.z);
                //Vector3 endPos = new Vector3(10.0f, 5.0f, 3.0f);
                Vector3 endVel = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 endAcc = new Vector3(0.0f, 0.0f, 0.0f);

              
                // Calculate minimum snap trajectory coefficients for each axis (x, y, z)
                coeffsX = MinimumSnapTrajectory.MinimumSnapCoefficients(startPos.x, startVel.x, startAcc.x, endPos.x, endVel.x, endAcc.x, T);
                coeffsY = MinimumSnapTrajectory.MinimumSnapCoefficients(startPos.y, startVel.y, startAcc.y, endPos.y, endVel.y, endAcc.y, T);
                coeffsZ = MinimumSnapTrajectory.MinimumSnapCoefficients(startPos.z, startVel.z, startAcc.z, endPos.z, endVel.z, endAcc.z, T);
                min_snap_flag = 2;
            }
         
            if(min_snap_flag == 2 && catching_time <= T)  
            {
                catching_time = catching_time + 0.1;
                double posX = MinimumSnapTrajectory.EvaluatePolynomial(coeffsX, catching_time);
                double posY = MinimumSnapTrajectory.EvaluatePolynomial(coeffsY, catching_time);
                double posZ = MinimumSnapTrajectory.EvaluatePolynomial(coeffsZ, catching_time);

                // Evaluate velocity (first derivative)
                double velX = MinimumSnapTrajectory.EvaluatePolynomialDerivative(coeffsX, catching_time);
                double velY = MinimumSnapTrajectory.EvaluatePolynomialDerivative(coeffsY, catching_time);
                double velZ = MinimumSnapTrajectory.EvaluatePolynomialDerivative(coeffsZ, catching_time);

                // Evaluate acceleration (second derivative)
                double accX = MinimumSnapTrajectory.EvaluatePolynomialSecondDerivative(coeffsX, catching_time);
                double accY = MinimumSnapTrajectory.EvaluatePolynomialSecondDerivative(coeffsY, catching_time);
                double accZ = MinimumSnapTrajectory.EvaluatePolynomialSecondDerivative(coeffsZ, catching_time);


                x_s_d = DenseVector.OfArray(new double[] { posX, posY, posZ });
                v_s_d = DenseVector.OfArray(new double[] { velX, velY, velZ });
                a_s_d = DenseVector.OfArray(new double[] { accX, accY, accZ });

            }
            
        
            /*
            Vector<double> buoy_w = R_ws*Rope.GetChild(Rope.childCount-1).position.To<NED>().ToDense();
            x_s_d = R_sw*DenseVector.OfArray(new double[] { buoy_w[0], buoy_w[1], Math.Pow(t-4, 2)/16 + buoy_w[2] + 0.16 });
            v_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, (t-4)/8 });
            a_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 1/8 });
            */
            //Debug.Log($"x_s_d: {x_s_d[0]:F2},{x_s_d[1]:F2},{x_s_d[2]:F2}"); // desired position
            // Debug.Log($"x_s: {x_s[0]:F2},{x_s[1]:F2},{x_s[2]:F2}"); // desired position
        }else
        {
            catching_time = 0; // reset time
        }
        // Debug.Log($"t: {t}"); // Time

        // Logging
        tw = new StreamWriter(filePath, true);
        tw.WriteLine($"{Time.time},{x_s[0]},{x_s[1]},{x_s[2]},{x_s_d[0]},{x_s_d[1]},{x_s_d[2]}");
        tw.Close();

        Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2)/2, -Math.Sqrt(2)/2, 0 });

        // Control
        Vector<double> ex = x_s - x_s_d;
        Vector<double> ev = v_s - v_s_d;

        Vector<double> pid = -kx*ex - kv*ev + mQ*g*e3 + mQ*a_s_d;
        Vector<double> b3d = pid/pid.Norm(2);
        Vector<double> b2d = _Cross(b3d, b1d)/_Cross(b3d, b1d).Norm(2);
        Vector<double> b1d_temp = _Cross(b2d, b3d);
        Matrix<double> R_sb_d = DenseMatrix.OfArray(new double[,] { { b1d_temp[0], b2d[0], b3d[0] },
                                                                    { b1d_temp[1], b2d[1], b3d[1] },
                                                                    { b1d_temp[2], b2d[2], b3d[2] } });
        
        Vector<double> W_b_d = _Vee(_Logm3(R_sb_d_prev.Transpose()*R_sb_d)/dt);
        Vector<double> W_b_d_dot = (W_b_d - W_b_d_prev)/dt;

        Vector<double> eR = 0.5*_Vee(R_sb_d.Transpose()*R_sb - R_sb.Transpose()*R_sb_d);
        Vector<double> eW = W_b - R_sb.Transpose()*R_sb_d*W_b_d;

        f = pid*(R_sb*e3);
        M = -kR*eR - kW*eW + _Cross(W_b, J*W_b) - J*(_Hat(W_b)*R_sb.Transpose()*R_sb_d*W_b_d - R_sb.Transpose()*R_sb_d*W_b_d_dot);

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
        double f;
        Vector<double> M;

        // If rope has been replaced (tension is high enough) use suspended load controller
        // If we have not hooked the rope yet, use normal tracking controller
        if (Rope != null && Rope.childCount == 2) (f, M) = SuspendedLoadControl();
        else (f, M) = TrackingControl();

        // Compute optimal propeller forces
        Vector<double> F_star = T_inv * DenseVector.OfArray(new double[] { f, M[0], M[1], M[2] });

        // Build a matrix A and a vector b to solve for the variation on the optimal propeller forces
        Matrix<double> A = Matrix<double>.Build.Dense(NUM_PROPS, NUM_PROPS);
        Vector<double> b = Vector<double>.Build.Dense(NUM_PROPS);
        for (int i = 0; i < NUM_PROPS; i++) {
            for (int j = 0; j < NUM_PROPS; j++) {
                if (F_star[i] >= 0 && F_star[j] >= 0) {
                    A[i, j] = Q[i, j];
                } else if (i == j) {
                    A[i, j] = 1;
                } else {
                    A[i, j] = 0;
                }
                if (F_star[i] >= 0 && F_star[j] < 0) {
                    b[i] += Q[i, j]*F_star[j];
                }
            }
            if (F_star[i] < 0) {
                b[i] = -F_star[i];
            }
        }
        Vector<double> F = F_star + A.Solve(b);

        // If the gradient tangent to any of the boundaries makes the solution not satisfy the constraints,
        // project it back to the boundary
        for (int i = 0; i < NUM_PROPS; i++) {
            if (F[i] < 0) {
                F[i] = 0;
            }
        }

        // Set propeller rpms
        for (int i = 0; i < propellers.Length; i++)
            propellers_rpms[i] = (float)F[i]/propellers[i].RPMToForceMultiplier;
	}

	void ApplyRPMs() 
    {
        // TODO: try clamping rpms to zero
        Debug.Log($"RPM: {propellers_rpms[0]:F2},{propellers_rpms[1]:F2},{propellers_rpms[2]:F2},{propellers_rpms[3]:F2}"); // desired position
        for (int i = 0; i < propellers.Length; i++) {
            if (propellers_rpms[i] < 0) {
                Debug.LogWarning("Propeller " + i + " has negative RPMs: " + propellers_rpms[i]);
                propellers_rpms[i] = 0;
            }
            propellers[i].SetRpm(propellers_rpms[i]);
        }
	}

    (Vector<double>, Vector<double>, Vector<double>) TrackingTargetTrajectory(Vector<double> x_TT, Vector<double> x_s, Vector<double> v_s) {
        Vector<double> x_s_d;
        Vector<double> v_s_d;
        Vector<double> a_s_d;
        
        Vector<double> unitVectorTowardsTarget = (x_TT - x_s)/(x_TT - x_s).Norm(2);
        // double accelerationDistance = Mathf.Pow(MaxVelocityWithTrackingTarget, 2)/(2*MaxAccelerationWithTrackingTarget);
        double distanceToTarget = (x_TT - x_s).Norm(2);
        // double velocityMagnitude;
        
        // // If we are not at the maximum velocity, we can accelerate
        // if (distanceToTarget > accelerationDistance && v_s.Norm(2) < MaxVelocityWithTrackingTarget) {
        //     if (startingPosition == null) {
        //         startingPosition = x_s - 1e-3*unitVectorTowardsTarget;
        //     }
        //     velocityMagnitude = Math.Sqrt(2*MaxAccelerationWithTrackingTarget*(x_s - startingPosition).Norm(2));
        //     x_s_d = x_s + velocityMagnitude*dt*unitVectorTowardsTarget;
        //     v_s_d = velocityMagnitude*unitVectorTowardsTarget;
        //     a_s_d = MaxAccelerationWithTrackingTarget*unitVectorTowardsTarget;
        // // If we want to move towards the target with maximum velocity
        // } else if (distanceToTarget > accelerationDistance) {
        //     startingPosition = null;
        //     velocityMagnitude = MaxVelocityWithTrackingTarget;
        //     x_s_d = x_s + velocityMagnitude*dt*unitVectorTowardsTarget;
        //     v_s_d = velocityMagnitude*unitVectorTowardsTarget;
        //     a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        // // If we are close to the target, slow down
        // } else if (distanceToTarget > 0.1) {
        //     startingPosition = null;
        //     velocityMagnitude = Math.Sqrt(2*MaxAccelerationWithTrackingTarget*distanceToTarget);
        //     x_s_d = x_s + velocityMagnitude*dt*unitVectorTowardsTarget;
        //     v_s_d = velocityMagnitude*unitVectorTowardsTarget;
        //     a_s_d = -MaxAccelerationWithTrackingTarget*unitVectorTowardsTarget;
        // // If we are at the target, stop
        // } else {
        //     startingPosition = null;
        //     x_s_d = x_TT;
        //     v_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        //     a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        // }

        // // If we are not at the maximum velocity, we can accelerate
        // if (distanceToTarget > accelerationDistance && v_s.Norm(2) < MaxVelocityWithTrackingTarget) {
        //     if (startingPosition == null) {
        //         startingPosition = x_s - 1e-3*unitVectorTowardsTarget;
        //     }
        //     velocityMagnitude = Math.Sqrt(2*MaxAccelerationWithTrackingTarget*(x_s - startingPosition).Norm(2));
        //     x_s_d = x_s + velocityMagnitude*dt*unitVectorTowardsTarget;
        // // If we want to move towards the target with maximum velocity
        // } else if (distanceToTarget > accelerationDistance) {
        //     startingPosition = null;
        //     velocityMagnitude = MaxVelocityWithTrackingTarget;
        //     x_s_d = x_s + velocityMagnitude*dt*unitVectorTowardsTarget;
        // // If we are close to the target, slow down
        // } else if (distanceToTarget > 0.1) {
        //     startingPosition = null;
        //     velocityMagnitude = Math.Sqrt(2*MaxAccelerationWithTrackingTarget*distanceToTarget);
        //     x_s_d = x_s + velocityMagnitude*dt*unitVectorTowardsTarget;
        // // If we are at the target, stop
        // } else {
        //     startingPosition = null;
        //     x_s_d = x_TT;
        // }
        // v_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        // a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });

        if (distanceToTarget > DecelerationDistance) {
            x_s_d = x_s + unitVectorTowardsTarget;
            v_s_d = MaxVelocityWithTrackingTarget*unitVectorTowardsTarget;
            a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        } else {
            x_s_d = x_TT;
            v_s_d = MaxVelocityWithTrackingTarget*distanceToTarget/DecelerationDistance*unitVectorTowardsTarget;
            a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        }

        return (x_s_d, v_s_d, a_s_d);
    }

    static Vector<double> _Cross(Vector<double> a, Vector<double> b) 
    {
        // Calculate each component of the cross product
        double c1 = a[1] * b[2] - a[2] * b[1];
        double c2 = a[2] * b[0] - a[0] * b[2];
        double c3 = a[0] * b[1] - a[1] * b[0];

        // Create a new vector for the result
        return DenseVector.OfArray(new double[] { c1, c2, c3 });
    }

    static Matrix<double> _Hat(Vector<double> v) 
    {
        return DenseMatrix.OfArray(new double[,] { { 0, -v[2], v[1] },
                                                   { v[2], 0, -v[0] },
                                                   { -v[1], v[0], 0 } });
    }
    
    static Vector<double> _Vee(Matrix<double> S) 
    {
        return DenseVector.OfArray(new double[] { S[2, 1], S[0, 2], S[1, 0] });
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
			m_ret = _Hat(Math.PI * omg);
			return m_ret;
		}
		else {
			double theta = Math.Acos(acosinput);
			m_ret = theta / 2.0 / Math.Sin(theta)*(R - R.Transpose());
			return m_ret;
		}
	}

    static Vector3 ToUnity(Vector<double> v) 
    {
        return new Vector3((float)v[0], (float)v[2], (float)v[1]);
    }
}



// Helper class for Minimum Snap Trajectory
public static class MinimumSnapTrajectory
{
    public static double[] MinimumSnapCoefficients(double startPos, double startVel, double startAcc,
                                                   double endPos, double endVel, double endAcc, 
                                                   double T)
    {
        var A = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            {1, 0, 0,    0,    0,    0},      
            {0, 1, 0,    0,    0,    0},      
            {0, 0, 2,    0,    0,    0},      
            {1, T, Math.Pow(T, 2), Math.Pow(T, 3), Math.Pow(T, 4), Math.Pow(T, 5)}, 
            {0, 1, 2*T,  3*Math.Pow(T, 2), 4*Math.Pow(T, 3), 5*Math.Pow(T, 4)},    
            {0, 0, 2,    6*T,  12*Math.Pow(T, 2), 20*Math.Pow(T, 3)}               
        });

        var b = Vector<double>.Build.Dense(new double[]
        {
            startPos, startVel, startAcc, endPos, endVel, endAcc
        });

        var x = A.Solve(b);

        return x.ToArray();
    }

    // Evaluate the polynomial at a given time t
    public static double EvaluatePolynomial(double[] coeffs, double t)
    {
        double result = 0;
        for (int i = 0; i < coeffs.Length; i++)
        {
            result += coeffs[i] * Math.Pow(t, i);
        }
        return result;
    }

    // Evaluate the first derivative (velocity) of the polynomial at time t
    public static double EvaluatePolynomialDerivative(double[] coeffs, double t)
    {
        double result = 0;
        for (int i = 1; i < coeffs.Length; i++)  // Start at i=1 because the derivative of a0 is 0
        {
            result += i * coeffs[i] * Math.Pow(t, i - 1);
        }
        return result;
    }

    // Evaluate the second derivative (acceleration) of the polynomial at time t
    public static double EvaluatePolynomialSecondDerivative(double[] coeffs, double t)
    {
        double result = 0;
        for (int i = 2; i < coeffs.Length; i++)  // Start at i=2 because the second derivative of a0 and a1 is 0
        {
            result += i * (i - 1) * coeffs[i] * Math.Pow(t, i - 2);
        }
        return result;
    }
