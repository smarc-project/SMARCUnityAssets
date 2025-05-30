using System;
using DefaultNamespace.LookUpTable;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using VehicleComponents.Actuators;

namespace DefaultNamespace
{
    public class BlueROV2ForceModel : MonoBehaviour
    {
        
        public ArticulationBody mainBody;
        public ArticulationBody prop_top_back_right;
        public ArticulationBody prop_top_front_right;
        public ArticulationBody prop_top_back_left;
        public ArticulationBody prop_top_front_left;
        public ArticulationBody prop_bot_back_right;
        public ArticulationBody prop_bot_front_right;
        public ArticulationBody prop_bot_back_left;
        public ArticulationBody prop_bot_front_left;

        public Propeller PropTopBackRight;
        public Propeller PropTopFrontRight;
        public Propeller PropTopBackLeft;
        public Propeller PropTopFrontLeft;
        public Propeller PropBotBackRight;
        public Propeller PropBotFrontRight;
        public Propeller PropBotBackLeft;
        public Propeller PropBotFrontLeft;
        
        
        //Variables
        public bool Ardusub_mode;
        public bool Arusub_prep;
        public bool Controller_mode = true;
        
        //Constants
        public double vbs = 0.0f; //some weird thing
        private double m = 0; //mass kg
        private double W = 0; //weight N
        private double B = 0; // bouyancy N
        double g = 9.82; // gravity m/s²
        double rho = 1000; // water density [kg/m^3]
        double nabla = 0.0134; // volume of BlueRoV [m^3], given experimental by OSBS
        private double rpmMax = 3000;
        
        //Bouyancy point coordinates relative to report coordinate system
        double  x_b = 0; double y_b = 0; double z_b = -0.01;
        
        //Added from OSBS
        //Rotational damping (Ns/m)
        double Xuu = 141; // #1.0
        double Yvv = 217; // #100.0
        double Zww = 190; // #100.0
        double Kpp = 1.19; // #10.0
        double Mqq = 0.47; // #100.0
        double Nrr = 1.5; // #150.0
        
        //Translational damping (Ns/m)
        double Xu = 13.7;
        double Yv = 0;
        double Zw = 33;
        double Kp = 0;
        double Mq = 0.8;
        double Nr = 0;
        
        // Added mass coefficients 
        double X_udot = 6.36; // [kg]
        double Y_vdot = 7.12; // [kg]
        double Z_wdot = 18.68; // [kg]
        double K_pdot = 0.189; // [kg*m^2]
        double M_qdot = 0.135; // [kg*m^2]
        double N_rdot = 0.222; // [kg*m^2]
        
        //Inertia 
        double I_x = 0.2818; // [kg*m^2], from OSBS's CAD
        double I_y = 0.245; // [kg*m^2], from OSBS's CAD
        double I_z = 0.3852; // [kg*m^2], from OSBS's CAD

        public void OnTickChange(bool tick)
        {
            Controller_mode = tick;
        }
        void Start()
        {
            // Get all propeller components
            PropTopBackRight = GameObject.Find("PropTopBackRight").GetComponent<Propeller>();
            PropTopFrontRight = GameObject.Find("PropTopFrontRight").GetComponent<Propeller>();
            PropTopBackLeft = GameObject.Find("PropTopBackLeft").GetComponent<Propeller>();
            PropTopFrontLeft = GameObject.Find("PropTopFrontLeft").GetComponent<Propeller>();
            PropBotBackRight = GameObject.Find("PropBotBackRight").GetComponent<Propeller>();
            PropBotFrontRight = GameObject.Find("PropBotFrontRight").GetComponent<Propeller>();
            PropBotBackLeft = GameObject.Find("PropBotBackLeft").GetComponent<Propeller>();
            PropBotFrontLeft = GameObject.Find("PropBotFrontLeft").GetComponent<Propeller>();
            
            // Get all propeller articulation bodies
            prop_top_back_right = GameObject.Find("prop_top_back_right_link").GetComponent<ArticulationBody>();
            prop_top_front_right = GameObject.Find("prop_top_front_right_link").GetComponent<ArticulationBody>();
            prop_top_back_left = GameObject.Find("prop_top_back_left_link").GetComponent<ArticulationBody>();
            prop_top_front_left = GameObject.Find("prop_top_front_left_link").GetComponent<ArticulationBody>();
            prop_bot_back_right = GameObject.Find("prop_bot_back_right_link").GetComponent<ArticulationBody>();
            prop_bot_front_right = GameObject.Find("prop_bot_front_right_link").GetComponent<ArticulationBody>();
            prop_bot_back_left = GameObject.Find("prop_bot_back_left_link").GetComponent<ArticulationBody>();
            prop_bot_front_left = GameObject.Find("prop_bot_front_left_link").GetComponent<ArticulationBody>();
            
            
            // Get mass from unity + one time calculations
            m = mainBody.mass; // mass 13.5
            I_x = mainBody.inertiaTensor.x;
            I_y = mainBody.inertiaTensor.z;
            I_z = mainBody.inertiaTensor.y; // y z switch. Unity to NED coordinates
            W = m * g; // weight
            B = rho*g*nabla; // The buoyancy in [N] given by OSBS
        }
        
        void FixedUpdate()
        {
            // Get world rotation
            var world_rot = mainBody.transform.rotation.eulerAngles; 
            var world_pos = mainBody.transform.position; 
            
            //Get and convert state vector from global to local reference point
            var inverseTransformDirection = mainBody.transform.InverseTransformDirection(mainBody.linearVelocity); // Local frame vel
            var transformAngularVelocity = mainBody.transform.InverseTransformDirection(mainBody.angularVelocity); // Local frame angular vel (gives negative velocities)
            
            // Convert angles, angular velocities and velocities to OSBS coordinate system
            var phiThetaTau = FRD.ConvertAngularVelocityFromRUF(world_rot).ToDense();
            float phi = (float) (Mathf.Deg2Rad * phiThetaTau[0]); 
            float theta = (float) (Mathf.Deg2Rad* phiThetaTau[1]);
            var uvw = inverseTransformDirection.To<NED>().ToDense(); // Might need to revisit. Rel. velocity in point m block.
            float u = (float) uvw[0];
            float v = (float) uvw[1];
            float w = (float) uvw[2];
            var pqr = FRD.ConvertAngularVelocityFromRUF(transformAngularVelocity).ToDense(); // FRD is same as NED for ANGLES ONLY
            float p = (float) pqr[0];
            float q = (float) pqr[1];
            float r = (float) pqr[2];
            
            // print(uvw[0]+","+uvw[1]+","+uvw[2]);
            // print(pqr[0]+","+pqr[1]+","+pqr[2]);    
        
            //State vector
            Vector<double> vel_vec = Vector<double>.Build.DenseOfArray(new double[] { u, v, w, p, q, r });
           
            // Rigid body and added mass matrices
            // Matrix<double> M_RB = DenseMatrix.OfDiagonalArray(new double[] {m, m, m, I_x, I_y, I_z});
            Matrix<double> M_A = DenseMatrix.OfDiagonalArray(new double[] {X_udot, Y_vdot, Z_wdot, K_pdot, M_qdot, N_rdot});
           
            // Coriollis and centripetal matrices
            Matrix<double> C_RB = DenseMatrix.OfArray(new double[,]
            {
                {0,     0,      0,      0,      m*w,    -m*v    },
                {0,     0,      0,      -m*w,   0,       m*u    },
                {0,     0,      0,      m*v,    -m*u,    0      },
                {0,     m*w,    -m*v,   0,      -I_z*r, -I_y*q  },
                {-m*w,  0,      m*u,    I_z*r,  0,       I_x*p  },
                {m*v,   -m*u,   0,      I_y*q,  -I_x*p,  0      },
            });
            Matrix<double> C_A = DenseMatrix.OfArray(new double[,]
            {
                {0,         0,          0,          0,          -Z_wdot*w,  Y_vdot*v    },
                {0,         0,          0,          Z_wdot*w,   0,          -X_udot*u   },
                {0,         0,          0,          -Y_vdot*v,  X_udot*u,   0           },
                {0,         -Z_wdot*w,  Y_vdot*v,   0,          -N_rdot*r,  M_qdot*q    },
                {Z_wdot*w,  0,          -X_udot*u,  N_rdot*r,   0,          -K_pdot*p   },
                {-Y_vdot*v, X_udot*u,   0,          -M_qdot*q,  K_pdot*p,   0           }
            });
            Matrix<double> C = C_RB + C_A;
            
            B = rho*g*nabla;
            if (world_pos.y >= 0)
            {
                B = 0;
            }
            
            // Restoring forces vector
            Vector<double> g_vec = Vector<double>.Build.DenseOfArray(new double[] 
                {
                (W-B)*Mathf.Sin(theta),
                -(W-B)*Mathf.Cos(theta)*Mathf.Sin(phi),
                -(W-B)*Mathf.Cos(theta)*Mathf.Cos(phi),
                y_b*B*Mathf.Cos(theta)*Mathf.Cos(phi)-z_b*B*Mathf.Cos(theta)*Mathf.Sin(phi),
                -z_b*B*Mathf.Sin(theta)-x_b*B*Mathf.Cos(theta)*Mathf.Cos(phi),
                x_b*B*Mathf.Cos(theta)*Mathf.Sin(phi)+y_b*B*Mathf.Sin(theta)
                }
            );
         
            // Dampening matrices
            Matrix<double> D = DenseMatrix.OfDiagonalArray(new double[]
            {
                Xu,
                Yv,
                Zw,
                Kp,
                Mq,
                Nr
            });
            Matrix<double> Dn = DenseMatrix.OfDiagonalArray(new double[] 
            {
                Xuu*Mathf.Abs(u),
                Yvv*Mathf.Abs(v), 
                Zww*Mathf.Abs(w),
                Kpp*Mathf.Abs(p),
                Mqq*Mathf.Abs(q), 
                Nrr*Mathf.Abs(r)
            });
            Matrix<double> D_of_vel = D + Dn;
            
            var v_c = 0; // Assume no ocean current. If desired to integrate it, info about it can be found in OSBS
            var vr = vel_vec - v_c;
            
            // Calculate dampening and coriolis forces
            var tau_sum_coriolis =  C * vel_vec;
            var tau_sum_damping = D_of_vel*vr; 

            // Separation into forces and torques
            var coriolisForce  = tau_sum_coriolis.SubVector(0, 3).ToVector3();
            var coriolisTorque = tau_sum_coriolis.SubVector(3, 3).ToVector3();
            var RestoringForce  = g_vec.SubVector(0, 3).ToVector3();
            var RestoringTorque = g_vec.SubVector(3, 3).ToVector3();
            var force_damping = tau_sum_damping.SubVector(0, 3).ToVector3();
            var torque_damping = tau_sum_damping.SubVector(3, 3).ToVector3();

            force_damping = NED.ConvertToRUF(force_damping);
            torque_damping = FRD.ConvertAngularVelocityToRUF(torque_damping);
            coriolisForce = NED.ConvertToRUF(coriolisForce);
            coriolisTorque = FRD.ConvertAngularVelocityToRUF(coriolisTorque);
            RestoringForce = NED.ConvertToRUF(RestoringForce);
            RestoringTorque = FRD.ConvertAngularVelocityToRUF(RestoringTorque);
            
            
            // Reset input forces every fixed update
            Vector3 inputForce = Vector3.zero;
            Vector3 inputTorque = Vector3.zero;
            
            // ROS Controlls
            // Update propeller rpm's
            float rpmTopBackRight = (float)PropTopBackRight.rpm;
            float rpmTopFrontRight = (float)PropTopFrontRight.rpm;
            float rpmTopBackLeft = (float)PropTopBackLeft.rpm;
            float rpmTopFrontLeft = (float)PropTopFrontLeft.rpm;

            float rpmBotBackRight = (float)PropBotBackRight.rpm;
            float rpmBotFrontRight = (float)PropBotFrontRight.rpm;
            float rpmBotBackLeft = (float)PropBotBackLeft.rpm;
            float rpmBotFrontLeft = (float)PropBotFrontLeft.rpm;
            
            // Define T matrix
            // Matrix<double> T = DenseMatrix.OfArray(new double[,]
            // {
            //     {-0.71, -0.71,  0.71,  0.71,  0,     0,    0,     0   },
            //     {0.71,  -0.71,  0.71, -0.71,  0,     0,    0,     0   },
            //     {0,      0,     0,     0,     1,     1,    1,     1   },
            //     {-0.06,  0.06, -0.06,  0.06,  0.22, -0.22, 0.22, -0.22},
            //     {-0.06, -0.06,  0.06,  0.06, -0.12, -0.12, 0.12,  0.12},
            //     {0.99,  -0.99, -0.99,  0.99,  0,     0,    0,     0   }
            // });
            
            Matrix<double> T = DenseMatrix.OfArray(new double[,]
            {
                { Math.Sqrt(2)/2,  Math.Sqrt(2)/2, -Math.Sqrt(2)/2, -Math.Sqrt(2)/2,  0,      0,       0,       0       },
                { -Math.Sqrt(2)/2, Math.Sqrt(2)/2, -Math.Sqrt(2)/2,  Math.Sqrt(2)/2,  0,      0,       0,       0       },
                { 0,               0,              0,               0,              -1,      1,       1,      -1       },
                { 0,               0,              0,               0,               0.218,  0.218,  -0.218,  -0.218   },
                { 0,                 0,              0,               0,               0.12,  -0.12,    0.12,   -0.12    },
                { -0.1888,         0.1888,         0.1888,         -0.1888,          0,      0,       0,       0       }
            });
            
            Vector<double> F_vec = Vector<double>.Build.DenseOfArray(new double[] 
                {
                    rpmBotFrontRight/rpmMax,
                    rpmBotFrontLeft/rpmMax,
                    rpmBotBackRight/rpmMax,
                    rpmBotBackLeft/rpmMax,
                    rpmTopFrontRight/rpmMax,
                    rpmTopFrontLeft/rpmMax,
                    rpmTopBackRight/rpmMax,
                    rpmTopBackLeft/rpmMax
                }
            );
            
            var ROSForces = T * F_vec;
            
            // print(F_vec[0]+","+F_vec[1]+","+F_vec[2]+","+F_vec[3]+","+F_vec[4]+","+F_vec[5]+","+F_vec[6]+","+F_vec[7]);    
            // if (Arusub_prep)
            // {
            //     Matrix<double> T_hat_inv = DenseMatrix.OfArray(new double[,]
            //     {
            //         { 0.25,  0.25, -0.25, -0.25,  0.0,  0.0,  0.0,  0.0 },
            //         { -0.25,  0.25, -0.25,  0.25,  0.0,  0.0,  0.0,  0.0 },
            //         { -0.0,  -0.0,  -0.0,   0.0,  -0.25, 0.25,  0.25, -0.25 },
            //         {  0.0,   0.0,   0.0,   0.0,   0.25, 0.25, -0.25, -0.25 },
            //         {  0.0,   0.0,   0.0,   0.0,   0.25, -0.25, 0.25, -0.25 },
            //         { -0.25,  0.25,  0.25, -0.25,  0.0,  0.0,  0.0,  0.0 }
            //     });
            //     
            //     ROSForces = T_hat_inv * F_vec;
            //    
            // }
            //
            //   // print("before ardusub");
            //   //           for (int i = 0; i < F_vec.Count; i++)
            //   //           {
            //   //               print(F_vec[i]);
            //   //           }
            //
            //
            //
            // if (Ardusub_mode)
            // {
            //     // print("thruster forces");
            //     // for (int i = 0; i < F_vec.Count; i++)
            //     // {
            //     //     F_vec[i] = VoltageToForce(F_vec[i]);
            //     //     print(F_vec[i]);
            //     // }
            //     Matrix<double> T_transpose = DenseMatrix.OfArray(new double[,]
            //     {
            //         {-1,  1,  0,  0,  0,  1},
            //         {-1, -1,  0,  0,  0, -1},
            //         { 1,  1,  0,  0,  0, -1},
            //         { 1, -1,  0,  0,  0,  1},
            //         { 0,  0, -1,  1, -1,  0},
            //         { 0,  0, 1, -1, -1,  0},
            //         { 0,  0, 1,  1,  1,  0},
            //         { 0,  0, -1, -1,  1,  0},
            //     });
            //     
            //     var F_vec_ardusub_unscaled = T_transpose * ROSForces;
            //     
            //     double[] yss = new double[4];
            //     double[] rph = new double[4];
            //     
            //     for (int i = 0; i < 4; i++)
            //     {
            //         yss[i] = F_vec_ardusub_unscaled[i];
            //         rph[i] = F_vec_ardusub_unscaled[i + 4];
            //     }
            //     
            //     double max_yss = 1;
            //     double max_rph = 1;
            //     
            //     for (int i = 1; i < 4; i++)
            //     {
            //         if (math.abs(yss[i]) > max_yss)
            //             max_yss = math.abs(yss[i]);
            //     
            //         if (math.abs(rph[i]) > max_rph)
            //             max_rph = math.abs(rph[i]);
            //     }
            //     
            //     for (int i = 0; i < 4; i++)
            //     {
            //         yss[i] /= max_yss;
            //         
            //         rph[i] /= max_rph;
            //     }   
            //     
            //     double[] adjust = { 1, 1, 1, 1, 1, 1, 1, 1 };
            //     
            //     Vector<double> F_vec_ardusub = Vector<double>.Build.DenseOfArray(new double[] 
            //         {
            //             (yss[0]*adjust[0]),
            //             (yss[1]*adjust[1]),
            //             (yss[2]*adjust[2]),
            //             (yss[3]*adjust[3]),
            //             (rph[0]*adjust[4]),
            //             (rph[1]*adjust[5]),
            //             (rph[2]*adjust[6]),
            //             (rph[3]*adjust[7]),
            //         }
            //     );
            //
            //     F_vec = F_vec_ardusub;
            // }
            
            // print("arduprepped thruster forces");
            for (int i = 0; i < F_vec.Count; i++)
            {
                // if (i < 4)
                // {
                //     F_vec[i] = -(F_vec[i]);
                // }
                F_vec[i] = VoltageToForce(F_vec[i]);
                // print(F_vec[i]);
            }
            
            ROSForces = T * F_vec;
            
            inputForce  = ROSForces.SubVector(0, 3).ToVector3();
            inputTorque = ROSForces.SubVector(3, 3).ToVector3();

            // Convert to keyboard format (unity coordinates)
            inputForce = NED.ConvertToRUF(inputForce);
            inputTorque = FRD.ConvertAngularVelocityToRUF(inputTorque);
           
            // debugging zero MPC input
            if (Controller_mode == false)
            {
                inputForce = Vector3.zero;
                inputTorque = Vector3.zero;

                // Keyboard controlls
                if (Input.GetKey(KeyCode.W))
                {
                    inputForce[2] += 86;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    inputForce[0] -= 85;
                }

                if (Input.GetKey(KeyCode.S))
                {
                    inputForce[2] -= 85;
                }

                if (Input.GetKey(KeyCode.D))
                {
                    inputForce[0] += 85;
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    inputForce[1] += 122;
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    inputForce[1] -= 122;
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    inputTorque[1] -= 14;
                }

                if (Input.GetKey(KeyCode.E))
                {
                    inputTorque[1] += 14;
                }

                if (Input.GetKey(KeyCode.X))
                {
                    inputTorque[0] += 14;
                }

                if (Input.GetKey(KeyCode.C))
                {
                    inputTorque[2] += 14;
                }
            }

            // inputForce = Vector3.zero;
            // inputTorque = Vector3.zero;
            // ADDED MASS
            var input_forces = inputForce.To<NED>().ToDense(); // Might need to revisit. Rel. velocity in point m block.
            var input_torques = FRD.ConvertAngularVelocityFromRUF(inputTorque).ToDense(); // FRD is same as NED for ANGLES ONLY (Negative since inputs are right handed )       
            var reactive_force_sum = (-g_vec - tau_sum_damping - tau_sum_coriolis);
            Vector<double> input_forces_sum  = Vector<double>.Build.DenseOfArray(new double[] {input_forces[0], input_forces[1], input_forces[2], input_torques[0], input_torques[1], input_torques[2] });
            var total_force_sum = reactive_force_sum + input_forces_sum;
            //print(input_forces[0]+","+input_forces[1]+","+input_forces[2]);
            Matrix<double> M_inv = DenseMatrix.OfDiagonalArray(new double[] // Inverted total mass matrix (rigid body + added mass)
            {
                0.0504,
                0.0485,
                0.0311,
                2.2272,
                2.7397,
                1.6892
            });
            //print(input_forces_sum[0]+","+input_forces_sum[1]+","+input_forces_sum[2]+","+input_forces_sum[3]+","+input_forces_sum[4]+","+input_forces_sum[5]);    

            var vel_vec_dot = M_inv*total_force_sum;
            var added_inertia = M_A * vel_vec_dot;
            var addedForce = added_inertia.SubVector(0, 3).ToVector3();
            var addedTorque = added_inertia.SubVector(3, 3).ToVector3();
            addedForce = NED.ConvertToRUF(addedForce);
            addedTorque = FRD.ConvertAngularVelocityToRUF(addedTorque);
            
            // ADD forces to rigid body 
            mainBody.AddRelativeForce(-force_damping);
            mainBody.AddRelativeForce(-coriolisForce);
            mainBody.AddRelativeForce(-RestoringForce);
            mainBody.AddRelativeForce(-addedForce);
            mainBody.AddRelativeForce(inputForce);
            mainBody.AddRelativeTorque(-torque_damping);
            mainBody.AddRelativeTorque(-coriolisTorque);
            mainBody.AddRelativeTorque(-RestoringTorque);
            mainBody.AddRelativeTorque(-addedTorque);
            mainBody.AddRelativeTorque(inputTorque);
            // added mass torque and force
            
            // Set RPMs for Visuals
            prop_top_back_right.SetDriveTargetVelocity(ArticulationDriveAxis.X, rpmTopBackRight);
            prop_top_front_right.SetDriveTargetVelocity(ArticulationDriveAxis.X, rpmTopFrontRight);
            prop_top_back_left.SetDriveTargetVelocity(ArticulationDriveAxis.X, rpmTopBackLeft);
            prop_top_front_left.SetDriveTargetVelocity(ArticulationDriveAxis.X, rpmTopFrontLeft);

            prop_bot_back_right.SetDriveTargetVelocity(ArticulationDriveAxis.Z, rpmBotBackRight);
            prop_bot_front_right.SetDriveTargetVelocity(ArticulationDriveAxis.Z, rpmBotFrontRight);
            prop_bot_back_left.SetDriveTargetVelocity(ArticulationDriveAxis.Z, rpmBotBackLeft);
            prop_bot_front_left.SetDriveTargetVelocity(ArticulationDriveAxis.Z, rpmBotFrontLeft);

            double VoltageToForce(double V)
            {
                double force = -140.3*math.pow(V,9)+389.9*math.pow(V,7)-404.1*math.pow(V,5)+176.0*math.pow(V,3)+8.9*V;
                return force;
            }
        }
    }
}
