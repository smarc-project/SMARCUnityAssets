using UnityEngine;
using VehicleComponents.Actuators;
using System;
using System.IO;
using System.Globalization;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using M350.PSDK_ROS2;
using VehicleComponents.Sensors;

namespace dji
{
    
    public enum ControllerType
    {
        FLU_Velocity,
        ENU_RelativePosition, //Not currently implemented
        FLU_Attitude,
    }

    public class DJIController : MonoBehaviour
    {
        public ArticulationBody ComAB;
        public ControllerType controllerType = ControllerType.FLU_Velocity;
        public Vector3 commandVelocityFLU = Vector3.zero;
        public Vector3 commandPositionENU = Vector3.zero;

        public Propeller FL, FR, BL, BR;

        public float comPitch, comRoll;
        public float comYaw;
        private Quaternion worldRotation;
        public Vector3 position;
        private Vector3 comVel;
        private Vector3 localVel;
        public Vector3 FLUVel;
        public float base_speed = 3000; //The "base" speed the props rotate at. Should be slightly below the critical speed that produces enough thrust to take off
        public float target_alt; //Height to hover at when in Attitude control mode
        public float command_pitch = 0; //target pitch when in Attitude control mode
        public float command_roll = 0; //target roll when in Attitude control mode
        public float command_yaw = 0; //target yaw, regardless of control mode
        private float target_pitch;
        private float target_roll;
        private float target_yaw;
        private PsdkTakeoffService takeoff_srv = null;
        private PsdkLandingService landing_srv = null;
        private LockedDirectionDepthSensor depthSensor = null;

        //Used for altitude controller
        private float prev_alt_error_pos = 0;
        private float prev_alt_output_pos = 0;
        private float alt_error_pos;

        //Used for vertical speed controller
        private float prev_alt_error_vel = 0;
        private float prev_alt_output_vel = 0;
        private float alt_error_vel;

        //Used for pitch controller
        private float prev_pitch_error = 0;
        private float prev_pitch_output = 0;
        private float pitch_error;

        //Used for roll controller
        private float prev_roll_error = 0;
        private float prev_roll_output = 0;
        private float roll_error;

        //Used for yaw controller
        private float prev_yaw_error = 0;
        private float prev_yaw_output = 0;
        public float yaw_error;
        public float yaw_output;

        //Metrics for forward speed controller 
        private float vel_k_forw = 30f;
        private float vel_z_forw = .9f;
        private float vel_p_forw = .8f;

        //Metrics for side-to-side speed controller
        private float vel_k_left =  20f;
        private float vel_z_left = 0f;
        private float vel_p_left = 0f;

        //Used for speed controller
        private Vector3 prev_vel_error = Vector3.zero;
        private Vector3 prev_vel_output = Vector3.zero;
        private Vector3 vel_output = Vector3.zero;
        private Vector3 vel_error;

        //Used to reduce the frequency of the speed controller for better performance
        private float counter = 0;
        private float counter_max = 25;

        //Path to which flight data is recorded. Should be a .csv
        public bool log_data = false;
        public string data_path = @"c:\School\UnityLogger.csv";

        public string robot_name = "M350/";

        public bool isTakingOff = false;
        public bool isLanding = false;
        public bool isLanded = true;

        public float max_controller_output_alt = 2000;
        public float max_controller_output_pitch_roll = 1000;
        private float max_RPM = 5200f;

        void OnValidate(){
            if(Mathf.Abs((float)(Time.fixedDeltaTime - 0.002)) < .0001){
                enabled = true;
            }
            else{
                enabled = false;
                if(Time.fixedDeltaTime > 0.002){
                    Debug.LogError("Timestep is too large! Set fixed time step to .002 s for DJI captain to be functional. Currently: " + Time.fixedDeltaTime);
                }
                if(Time.fixedDeltaTime < 0.002){
                    Debug.LogError("Timestep is too small! Set fixed time step to .002 s for DJI captain to be functional or edit DJI captain to downsample to .002s. Currently: " + Time.fixedDeltaTime);
                }
            }
        }

        void Awake(){
            FL.HoverDefault = false;
            BL.HoverDefault = false;
            FR.HoverDefault = false;
            BR.HoverDefault = false;
        }

        void Start(){
            if(!enabled){
                return;
            }
            if(takeoff_srv == null){
                takeoff_srv = GetComponentInChildren<PsdkTakeoffService>();
            }
            if(landing_srv == null){
                landing_srv = GetComponentInChildren<PsdkLandingService>();
            }
            if(depthSensor == null){
                depthSensor = GetComponentInChildren<LockedDirectionDepthSensor>();
            }
            if(log_data){
                DateTime localDate = DateTime.Now;
                DateTime utcDate = DateTime.UtcNow;
                if (!File.Exists(data_path))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(data_path))
                    {
                        sw.WriteLine("NEW, {0}", localDate.ToString(new CultureInfo("en-GB")));
                        sw.WriteLine("Time, Roll, Pitch, Yaw, Target Roll, Target Pitch, Target Yaw, VelForw, VelLeft, VelUp, Target VelForw, Target VelLeft, Target VelUp, FL, FR, BL, BR, Target Pitch, Target Roll");
                    }
                }
                else{
                    using (StreamWriter sw = File.AppendText(data_path)){
                        sw.WriteLine("NEW, {0}", localDate.ToString(new CultureInfo("en-GB")));
                        sw.WriteLine("Time, Roll, Pitch, Yaw, Target Roll, Target Pitch, Target Yaw, VelForw, VelLeft, VelUp, Target VelForw, Target VelLeft, Target VelUp, FL, FR, BL, BR, Target Pitch, Target Roll");
                    }
                }
            }
        }
 
        void FixedUpdate()
        {
            if(!enabled){
                return;
            }
            if(isLanded){
                FL.SetRpm(0);
                FR.SetRpm(0);
                BL.SetRpm(0);
                BR.SetRpm(0);
                return;
            }
            counter++;

            //Current rotation about the center of mass
            comPitch = ComAB.transform.rotation.eulerAngles.z;
            comRoll = ComAB.transform.rotation.eulerAngles.x;
            comYaw = ComAB.transform.rotation.eulerAngles.y;

            worldRotation = ComAB.transform.rotation;
            comVel = ComAB.linearVelocity;
            localVel = Quaternion.Inverse(worldRotation) * comVel;
            FLUVel.y = localVel.z;
            FLUVel.x = localVel.x;
            FLUVel.z = localVel.y;
            position = ComAB.transform.position;
            vel_error = Vector3.zero;

            if(takeoff_srv == null){
                takeoff_srv = GetComponentInChildren<PsdkTakeoffService>();
            }
            if(landing_srv == null){
                landing_srv = GetComponentInChildren<PsdkLandingService>();
            }
            if(depthSensor == null){
                depthSensor = GetComponentInChildren<LockedDirectionDepthSensor>();
            }

            target_yaw = command_yaw; //target and command angles are split so that the same attitude control can be used for velocity. For yaw, these are currently always equivalent.

            //Vertical Controllers
            float alt_output;
            if(controllerType == ControllerType.FLU_Attitude || isTakingOff || isLanding){
                //Altitude Controller. This is used either in Attitude Control Mode or while taking off or landing.
                alt_error_pos = target_alt - position.y;

                float k_alt_pos = 2712f; 
                float z_alt_pos = .999263f;
                float p_alt_pos = .9937f;

                alt_output = k_alt_pos * alt_error_pos - k_alt_pos * z_alt_pos * prev_alt_error_pos + p_alt_pos * prev_alt_output_pos; //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
                if(alt_output > max_controller_output_alt){
                    alt_output = max_controller_output_alt;
                }
                else if(alt_output < -100){
                    alt_output = -100;
                }
                prev_alt_error_pos = alt_error_pos;
                prev_alt_output_pos = alt_output;

                if(isTakingOff){ //Checks if done taking off
                    if(position.y > takeoff_srv.takeoffAlt - takeoff_srv.takeoffError){
                        isTakingOff = false;
                        Debug.Log("Setting takeoff to false");
                    }
                }

                if(isLanding){
                    Debug.Log("Depth: " + depthSensor.depth + " landingError: " + landing_srv.landingError);
                    if(depthSensor.depth > landing_srv.landingError){
                        target_alt = position.y - depthSensor.depth;
                    }
                    else{
                        isLanding = false;
                        Debug.Log("Setting landing to false");
                        if(!depthSensor.usingWater){
                            isLanded = true;
                        }
                        
                    }
                }
            }

            else{
                //Vertical Speed controller 
                alt_error_vel = commandVelocityFLU.z - FLUVel.z;

                float k_alt_vel = 3000f;
                float z_alt_vel = .85f;
                float p_alt_vel = .9f;

                alt_output = k_alt_vel * alt_error_vel - k_alt_vel * z_alt_vel * prev_alt_error_vel + p_alt_vel * prev_alt_output_vel; //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
                if(alt_output > max_controller_output_alt){
                    alt_output = max_controller_output_alt;
                }
                else if(alt_output < -max_controller_output_alt){
                    alt_output = -max_controller_output_alt;
                }
                prev_alt_error_vel = alt_error_vel;
                prev_alt_output_vel = alt_output;
            }

            if(controllerType == ControllerType.FLU_Velocity){
                vel_error = FLUVel - commandVelocityFLU;
            }

            if(controllerType == ControllerType.FLU_Attitude){
                target_pitch = command_pitch;
                target_roll = command_roll;
                vel_output = Vector3.zero;
            }

            else if(counter >= counter_max){
                //if statement used to reduce the frequency of the velocity controller. This is done to improve the performance by making it slower than the attitude controller.
                counter = 0;
                target_pitch = vel_error.x * vel_k_forw - prev_vel_error.x * vel_k_forw * vel_z_forw + prev_vel_output.x * vel_p_forw; //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
                target_roll = -(vel_error.y * vel_k_left - prev_vel_error.y * vel_k_left * vel_z_left + prev_vel_output.y * vel_p_left); //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
                prev_vel_error = vel_error;
                
                //Roll and pitch limited to 20 degrees. This is partly for safety and partly because the controller was designed around a linear approximation of sine.
                if(target_pitch > 20){
                    target_pitch = 20;
                }
                else if(target_pitch < -20){
                    target_pitch = -20;
                }
                
                if(target_roll > 20){
                    target_roll = 20;
                }
                else if(target_roll < -20){
                    target_roll = -20;
                }

                prev_vel_output.x = target_pitch;
                prev_vel_output.y = target_roll;

            }

            //Pitch controller
            pitch_error = target_pitch - comPitch;
            if (pitch_error > 180f) pitch_error -= 360f;
            if (pitch_error < -180f) pitch_error += 360f;

            float k = 1200f; //Larger makes faster
            float z = 0.98f; //Larger makes mores stable (between 0 and 1)
            float p = .95f; //Larger makes "more aggressive", faster with more overshoot (between 0 and 1)

            var pitch_output = k * pitch_error - k * z * prev_pitch_error + p * prev_pitch_output; //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
            if(pitch_output > max_controller_output_pitch_roll){
                pitch_output = max_controller_output_pitch_roll; //This is in prop rpm. Limiting to avoid physical and sim max speeds 
            }
            else if(pitch_output < -max_controller_output_pitch_roll){
                pitch_output = -max_controller_output_pitch_roll;
            } 
            prev_pitch_error = pitch_error;
            prev_pitch_output = pitch_output;

            roll_error = target_roll - comRoll;
            if (roll_error > 180f) roll_error -= 360f;
            if (roll_error < -180f) roll_error += 360f;

            //Roll Controller
            k = 1200f;
            z = .98f;
            p = .95f;

            var roll_output = k * roll_error - k * z * prev_roll_error + p * prev_roll_output; //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
            if(roll_output > max_controller_output_pitch_roll){
                roll_output = max_controller_output_pitch_roll;
            }
            else if(roll_output < -max_controller_output_pitch_roll){
                roll_output = -max_controller_output_pitch_roll;
            }
            prev_roll_error = roll_error;
            prev_roll_output = roll_output;

            //Yaw Controller
            yaw_error = target_yaw - comYaw;
            if (yaw_error > 180f) yaw_error -= 360f;
            if (yaw_error < -180f) yaw_error += 360f;

            float k_yaw = 8f;
            float z_yaw = .98f;
            float p_yaw = .8f;

            yaw_output = k_yaw * yaw_error - k_yaw * z_yaw * prev_yaw_error + p_yaw * prev_yaw_output ; //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
            if(yaw_output > 500){
                yaw_output = 500;
            }
            else if(yaw_output < -500){
                yaw_output = -500;
            }
            prev_yaw_error = yaw_error;
            prev_yaw_output = yaw_output;

            var FL_RPM = base_speed + alt_output + pitch_output - roll_output - yaw_output;
            var FR_RPM = base_speed + alt_output + pitch_output + roll_output + yaw_output;
            var BL_RPM = base_speed + alt_output - pitch_output - roll_output + yaw_output;
            var BR_RPM = base_speed + alt_output - pitch_output + roll_output - yaw_output;

            if(FL_RPM > max_RPM || FR_RPM > max_RPM || BL_RPM > max_RPM || BR_RPM > max_RPM || FL_RPM < 0 || FR_RPM < 0 || BL_RPM < 0 || BR_RPM < 0){
                Debug.LogWarning("Target RPM outside bounds! FL: " + FL_RPM + " FR: " + FR_RPM + " BL: " + BL_RPM + " BR: " + BR_RPM);
                var available = max_RPM - (base_speed + alt_output);
                float desired;
                if(FL_RPM > max_RPM || FR_RPM > max_RPM || BL_RPM > max_RPM || BR_RPM > max_RPM){
                    if(FL_RPM > max_RPM){
                        desired = FL_RPM - (base_speed + alt_output);
                    }
                    else if(FR_RPM > max_RPM){
                        desired = FR_RPM - (base_speed + alt_output);
                    }
                    else if(BL_RPM > max_RPM){
                        desired = BL_RPM - (base_speed + alt_output);
                    }
                    else{
                        desired = BR_RPM - (base_speed + alt_output);
                    }
                    pitch_output *= available / desired;
                    roll_output *= available / desired;
                    yaw_output *= available / desired;

                    prev_yaw_output = yaw_output;
                    prev_pitch_output = pitch_output;
                    prev_roll_output = roll_output;

                    FL_RPM = base_speed + alt_output + pitch_output - roll_output - yaw_output;
                    FR_RPM = base_speed + alt_output + pitch_output + roll_output + yaw_output;
                    BL_RPM = base_speed + alt_output - pitch_output - roll_output + yaw_output;
                    BR_RPM = base_speed + alt_output - pitch_output + roll_output - yaw_output;
                    if(FL_RPM > max_RPM || FR_RPM > max_RPM || BL_RPM > max_RPM || BR_RPM > max_RPM){
                        Debug.LogWarning("Target RPM still too high! FL: " + FL_RPM + " FR: " + FR_RPM + " BL: " + BL_RPM + " BR: " + BR_RPM);
                    }
                }
            }
            
            FL.SetRpm(FL_RPM);
            FR.SetRpm(FR_RPM);
            BL.SetRpm(BL_RPM);
            BR.SetRpm(BR_RPM);
            if(log_data){
                using (StreamWriter sw = File.AppendText(data_path)){
                        sw.WriteLine($"{Time.fixedTime},{comRoll},{comPitch},{comYaw},{target_roll},{target_pitch},{target_yaw},{FLUVel.x},{FLUVel.y},{FLUVel.z},{commandVelocityFLU.x},{commandVelocityFLU.y},{commandVelocityFLU.z}, {FL_RPM}, {FR_RPM}, {BL_RPM}, {BR_RPM}, {target_pitch}, {target_roll}");
                }
            }
        }
    }
}