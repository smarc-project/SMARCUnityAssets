using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.Core; //Clock
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using DefaultNamespace.LookUpTable;
using VehicleComponents.Actuators;
using Rope;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class DroneLoadController: MonoBehaviour {
    public GameObject base_link;
    public GameObject load_link; // TODO: For now the position of the AUV is taken at the base of the rope
    public GameObject hook_link;
    public float computation_frequency = 50f;
    public bool follow_tracking_target = false;//
    public Transform tracking_target_transform;
	private Propeller[] propellers;
    private float[] propellers_rpms;
    private ArticulationBody base_link_ab;
    private ArticulationBody load_link_ab;
    private Matrix<double> R_sb_d_prev;
    private Matrix<double> R_sb_c_prev;
    private Vector<double> W_b_d_prev;
    private Vector<double> W_b_c_prev;
    private Vector<double> q_c_prev;
    private Vector<double> q_c_dot_prev;
    private int times1 = 0;
    private int times2 = 0;
    private GameObject rope;

    public double desiredY = 10;
    public GameObject AUV_gameobject;
    public double desiredDisplacement = 10;

     private bool isCallbackReceived = false; 

    // ROS Connector
    private ROSConnection ros;
    public string buoyPositionTopic = "/drone/georeferenced_point";
    protected PointMsg callbackMsg;
    
    private Vector3 buoyPosition = Vector3.zero;
    private Vector3 trajStart;
    private int i = 0;
    public float alpha = 0.12f;


    // Quadrotor parameters
    double mQ;
    double d;
    Matrix<double> J;
    float c_tau_f;

    // Load parameters
    double mL;
    double l;

    // Simulation parameters
    double g;
    Vector<double> e3;
    double dt;
    float t;
    float t_buoy;

    // Gains
    double kx;
    double kv;
    double kR;
    double kW;
    double kq;
    double kw;

	// Use this for initialization
	void Start() {
        hook_link.SetActive(false);
		propellers = new Propeller[4];
		propellers[0] = GameObject.Find("propeller_FL").GetComponent<Propeller>();
		propellers[1] = GameObject.Find("propeller_FR").GetComponent<Propeller>();
        propellers[2] = GameObject.Find("propeller_BR").GetComponent<Propeller>();
        propellers[3] = GameObject.Find("propeller_BL").GetComponent<Propeller>();

        base_link_ab = base_link.GetComponent<ArticulationBody>();

        R_sb_d_prev = DenseMatrix.OfArray(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        R_sb_c_prev = DenseMatrix.OfArray(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        W_b_d_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        W_b_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });
        q_c_dot_prev = DenseVector.OfArray(new double[] { 0, 0, 0 });

		propellers_rpms = new float[] { 0, 0, 0, 0 };

        rope = GameObject.Find("Rope");
        load_link_ab = load_link.GetComponent<ArticulationBody>();
        
        // Quadrotor parameters
        mQ = base_link_ab.mass + 0.026;
        d = 0.315;
        J = DenseMatrix.OfArray(new double[,] { { base_link_ab.inertiaTensor.x, 0, 0 }, { 0, base_link_ab.inertiaTensor.z, 0 }, { 0, 0, base_link_ab.inertiaTensor.y } });
        c_tau_f = 8.004e-4f;

        // Load parameters
        mL = 12.012 + 2.7 + 0.3;
        l = 1;

        // Simulation parameters
        g = 9.81;
        e3 = DenseVector.OfArray(new double[] { 0, 0, 1 });
        dt = 1f/computation_frequency;

        // Initialize ROS
        callbackMsg = new PointMsg();
        ros = ROSConnection.GetOrCreateInstance();
        // ros.Subscribe<PointMsg>(buoyPositionTopic, UpdateBuoyPosition);
        ros.Subscribe<Float32MultiArrayMsg>("/drone/georeferenced_coordinates", ReceiveMessage);
        // InvokeRepeating("ComputeRPMs", 0f, dt);
	}

    private void ReceiveMessage(Float32MultiArrayMsg message)
    {
        i++;
        if(i==20){
            // isCallbackReceived = true;
            trajStart = base_link_ab.transform.position ;
            t_buoy = Time.time;
            // hook_link.SetActive(true);
        }
        if(!isCallbackReceived)
        {
        // Process the Float32MultiArray message
        // Debug.Log("Received message with " + message.data.Length + " elements.");
        Vector3 value = new Vector3(message.data[0], message.data[2], message.data[1]);
        // Debug.Log("Value: " + value);
        buoyPosition = new Vector3((float)value.x, (float)value.y, (float)value.z);
        Debug.DrawLine(value, base_link_ab.transform.position, Color.red);
        }
    }

    // void UpdateBuoyPosition(PointMsg pointMsg)
    // {
    //     callbackMsg = pointMsg;
    //     // Update the buoy position from the ROS Point message
    //     buoyPosition = new Vector3((float)callbackMsg.x, (float)callbackMsg.y, (float)callbackMsg.z);
    //     // Set the callback flag to true
    //     isCallbackReceived = true;
    //     Debug.Log("recieved buoy estimate");
    // }
	
	// Update is called once per frame
	void FixedUpdate() {
		ComputeRPMs();
        ApplyRPMs();
	}

	void ComputeRPMs() {
        t = Time.time;

        double f;
        Vector<double> M;
        if (false){//(rope.transform.childCount == 2) {          
            // Debug.Break();
            
            // Gains
            kx = 2;
            kv = 1;
            kR = 10;
            kW = 0.5;
            kq = 2;
            kw = 0.5;
            
            // Quadrotor states
            Vector<double> xQ_s = base_link.transform.position.To<ENU>().ToDense();
            Vector<double> vQ_s = base_link_ab.velocity.To<ENU>().ToDense();
            Matrix<double> R_sb = DenseMatrix.OfArray(new double[,] { { base_link.transform.right.x, base_link.transform.forward.x, base_link.transform.up.x },
                                                                    { base_link.transform.right.z, base_link.transform.forward.z, base_link.transform.up.z },
                                                                    { base_link.transform.right.y, base_link.transform.forward.y, base_link.transform.up.y } });
            Vector<double> W_b = -1f*(base_link.transform.InverseTransformDirection(base_link_ab.angularVelocity)).To<ENU>().ToDense();

            // Load states
            Vector<double> xL_s = load_link.transform.position.To<ENU>().ToDense();
            Vector<double> vL_s = load_link_ab.velocity.To<ENU>().ToDense();
            Vector<double> q = (xL_s - xQ_s)/l;
            Vector<double> q_dot = (vL_s - vQ_s)/l;

            // Desired states
            Vector<double> xL_s_d;//Math.Pow(0.5*t-5, 2) });
            Vector<double> vL_s_d;//0.5*t-5 });
            Vector<double> aL_s_d;//0.5 });
            // if (follow_tracking_target) {
            //     xL_s_d = tracking_target_transform.position.To<ENU>().ToDense();
            //     vL_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
            //     aL_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
            // } else {
            xL_s_d = DenseVector.OfArray(new double[] { 0, 0, 5 });
            vL_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
            aL_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
        // }
            // Figure 8:
            // { 0, 2*Math.Sin(t), 3*Math.Cos(0.5*t) + 25 }
            // { 0, 2*Math.Cos(t), -1.5*Math.Sin(0.5*t) }
            // { 0, -2*Math.Sin(t), -0.75*Math.Cos(0.5*t) }
            
            Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2)/2, -Math.Sqrt(2)/2, 0 });

            // Load position controller
            Vector<double> ex = xL_s - xL_s_d;
            Vector<double> ev = vL_s - vL_s_d;
            Debug.Log($"ex: {ex}, ev: {ev}");

            Vector<double> A = -kx*ex - kv*ev + (mQ+mL)*(aL_s_d + g*e3) + mQ*l*(q_dot*q_dot)*q;
            Vector<double> q_c = -A/A.Norm(2);
            Vector<double> q_c_dot = DenseVector.OfArray(new double[] { 0, 0, 0 });//(q_c - q_c_prev)/dt;
            Vector<double> q_c_ddot = DenseVector.OfArray(new double[] { 0, 0, 0 });//(q_c_dot - q_c_dot_prev)/dt;
            Vector<double> F_n = (A*q)*q;

            // Load attitude controller
            Vector<double> eq = _Hat(q)*_Hat(q)*q_c;
            Vector<double> eq_dot = q_dot - _Cross(_Cross(q_c, q_c_dot), q);
            
            Vector<double> F_pd = -kq*eq - kw*eq_dot;
            Vector<double> F_ff = mQ*l*(q*_Cross(q_c, q_c_dot))*_Cross(q, q_dot) + mQ*l*_Cross(_Cross(q_c, q_c_ddot), q);
            Vector<double> F_for_f = F_n - F_pd - F_ff;
            Debug.DrawRay(ToUnity(xQ_s), ToUnity(F_for_f), Color.green);
            Debug.DrawRay(ToUnity(xQ_s), ToUnity(q_c), Color.magenta);
            // Debug.DrawRay(ToUnity(xQ_s), ToUnity(-F_ff), Color.yellow);
            
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

            if (times1 < 2) {
                times1++;
                f = 0;
                M = DenseVector.OfArray(new double[] { 0, 0, 0 });
            }

        } else {
            // Gains
            kx = 16*mQ;
            kv = 5.6*mQ;
            kR = 8.81;
            kW = 2.54;
            
            // Quadrotor states
            Vector<double> x_s = base_link.transform.position.To<NED>().ToDense();
            Vector<double> v_s = base_link_ab.velocity.To<NED>().ToDense();
            Matrix<double> R_wa = DenseMatrix.OfArray(new double[,] { { base_link.transform.right.x, base_link.transform.forward.x, base_link.transform.up.x },
                                                                    { base_link.transform.right.z, base_link.transform.forward.z, base_link.transform.up.z },
                                                                    { base_link.transform.right.y, base_link.transform.forward.y, base_link.transform.up.y } });
            Vector<double> W_b = FRD.ConvertAngularVelocityFromRUF(base_link.transform.InverseTransformDirection(base_link_ab.angularVelocity)).ToDense();

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
            Vector<double> buoy_w = R_ws*rope.transform.GetChild(rope.transform.childCount-1).position.To<NED>().ToDense();
            Vector<double> x_s_d;
            Vector<double> v_s_d;
            Vector<double> a_s_d;

            // if (follow_tracking_target) {
            //     x_s_d = tracking_target_transform.position.To<NED>().ToDense();
            //     v_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
            //     a_s_d = DenseVector.OfArray(new double[] { 0, 0, 0 });
            // }
            if (!isCallbackReceived) {
                x_s_d = R_sw*DenseVector.OfArray(new double[] { (0.25*t + AUV_gameobject.transform.position.x - desiredDisplacement), AUV_gameobject.transform.position.z, desiredY/Math.Pow(desiredDisplacement,2)*Math.Pow(0.25*t-desiredDisplacement, 2)+5});
                v_s_d = R_sw*DenseVector.OfArray(new double[] { 0.25, 0, desiredY/Math.Pow(desiredDisplacement,2)*2*(0.25*t-desiredDisplacement) });
                a_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, desiredY/Math.Pow(desiredDisplacement,2)*2*0.25});
            } 
            else {
                float t_dash = t - t_buoy;
                Debug.Log("resetted time" + t_dash);
                // x_s_d = R_sw*DenseVector.OfArray(new double[] { buoyPosition[0], buoyPosition[2], Math.Pow(t_dash-4, 2)/16 + buoyPosition[1] + 0.16 });
                // v_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, (t_dash-4)/8 });
                // a_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, 1/8 });

                x_s_d = R_sw*DenseVector.OfArray(new double[] { (alpha*t_dash + buoyPosition[0] - desiredDisplacement/3), buoyPosition[2], desiredY/2/Math.Pow(desiredDisplacement/3,2)*Math.Pow(alpha*t_dash-desiredDisplacement/3, 2)+0.4});
                v_s_d = R_sw*DenseVector.OfArray(new double[] { alpha, 0, desiredY/2/Math.Pow(desiredDisplacement/3,2)*2*(alpha*t_dash-desiredDisplacement/3) });
                a_s_d = R_sw*DenseVector.OfArray(new double[] { 0, 0, desiredY/2/Math.Pow(desiredDisplacement/3,2)*2*alpha});
            }
            Vector<double> b1d = DenseVector.OfArray(new double[] { Math.Sqrt(2)/2, -Math.Sqrt(2)/2, 0 });//

            // Control
            Vector<double> ex = x_s - x_s_d;
            Vector<double> ev = v_s - v_s_d;

            Vector<double> pid = -kx*ex - kv*ev - (mQ + (rope.transform.childCount == 2 ? mL : 0))*g*e3 + mQ*a_s_d;
            Vector<double> b3d = -pid/pid.Norm(2);
            Vector<double> b2d = _Cross(b3d, b1d)/_Cross(b3d, b1d).Norm(2);
            Vector<double> b1d_temp = _Cross(b2d, b3d);
            Matrix<double> R_sb_d = DenseMatrix.OfArray(new double[,] { { b1d_temp[0], b2d[0], b3d[0] },
                                                                        { b1d_temp[1], b2d[1], b3d[1] },
                                                                        { b1d_temp[2], b2d[2], b3d[2] } });
            
            Vector<double> W_b_d = _Vee(_Logm3(R_sb_d_prev.Transpose()*R_sb_d)/dt);
            Vector<double> W_b_d_dot = (W_b_d - W_b_d_prev)/dt;

            Vector<double> eR = 0.5*_Vee(R_sb_d.Transpose()*R_sb - R_sb.Transpose()*R_sb_d);
            Vector<double> eW = W_b - R_sb.Transpose()*R_sb_d*W_b_d;

            f = -pid*(R_sb*e3);
            M = -kR*eR - kW*eW + _Cross(W_b, J*W_b) - J*(_Hat(W_b)*R_sb.Transpose()*R_sb_d*W_b_d - R_sb.Transpose()*R_sb_d*W_b_d_dot);

            R_sb_d_prev = R_sb_d;
            W_b_d_prev = W_b_d;

            if (times2 < 2 || M.Norm(2) > 100) {
                times2++;
                f = 0;
                M = DenseVector.OfArray(new double[] { 0, 0, 0 });
            }
            // Debug.Log($"R_sb: {R_sb}, R_sb_d: {R_sb_d} R_sb_d_prev: {R_sb_d_prev}");
        }

        // Convert to propeller forces
        Matrix<double> T = DenseMatrix.OfArray(new double[,] { { 1, 1, 1, 1 }, { 0, -d, 0, d }, { d, 0, -d, 0 }, { -c_tau_f, c_tau_f, -c_tau_f, c_tau_f } });
        Vector<double> F = T.Inverse() * DenseVector.OfArray(new double[] { f, M[0], M[1], M[2] });

        // Debug.Log($"f: {f}, M: {M}");

        // Set propeller rpms
        propellers_rpms[0] = (float)F[0]/0.005f;
        propellers_rpms[1] = (float)F[1]/0.005f;
        propellers_rpms[2] = (float)F[2]/0.005f;
        propellers_rpms[3] = (float)F[3]/0.005f;
	}

	void ApplyRPMs() {
		for (int i = 0; i < 4; i++) {
            propellers[i].SetRpm(propellers_rpms[i]);
        }
	}

    static Vector<double> _Cross(Vector<double> a, Vector<double> b) {
        // Calculate each component of the cross product
        double c1 = a[1] * b[2] - a[2] * b[1];
        double c2 = a[2] * b[0] - a[0] * b[2];
        double c3 = a[0] * b[1] - a[1] * b[0];

        // Create a new vector for the result
        return DenseVector.OfArray(new double[] { c1, c2, c3 });
    }

    static Matrix<double> _Hat(Vector<double> v) {
        return DenseMatrix.OfArray(new double[,] { { 0, -v[2], v[1] },
                                                   { v[2], 0, -v[0] },
                                                   { -v[1], v[0], 0 } });
    }
    
    static Vector<double> _Vee(Matrix<double> S) {
        return DenseVector.OfArray(new double[] { S[2, 1], S[0, 2], S[1, 0] });
    }

    static Matrix<double> _Logm3(Matrix<double> R) {
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

    static Vector3 ToUnity(Vector<double> v) {
        return new Vector3((float)v[0], (float)v[2], (float)v[1]);
    }
}