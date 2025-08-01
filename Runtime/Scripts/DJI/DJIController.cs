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
        private PsdkTakeoffService takeoff_srv;

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

        void Awake(){
            FL.HoverDefault = false;
            BL.HoverDefault = false;
            FR.HoverDefault = false;
            BR.HoverDefault = false;
        }

        void Start(){
            Debug.LogWarning("Currently running DJIController. Make sure Fixed Time Step is set to 0.002 in Project Settings or Controller will not work!");
            if(takeoff_srv == null){
                takeoff_srv = GetComponentInChildren<PsdkTakeoffService>();
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

            target_yaw = command_yaw; //target and command angles are split so that the same attitude control can be used for velocity. For yaw, these are currently always equivalent.

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
            if(pitch_output > 1000){
                pitch_output = 1000; //This is in prop rpm. Limiting to avoid physical and sim max speeds 
            }
            else if(pitch_output < -1000){
                pitch_output = -1000;
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
            if(roll_output > 1000){
                roll_output = 1000;
            }
            else if(roll_output < -1000){
                roll_output = -1000;
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
            if(yaw_output > 100){
                yaw_output = 100;
            }
            else if(yaw_output < -100){
                yaw_output = -100;
            }
            prev_yaw_error = yaw_error;
            prev_yaw_output = yaw_output;

            //Vertical Controllers
            float alt_output;
            if(controllerType == ControllerType.FLU_Attitude || (takeoff_srv != null && takeoff_srv.takingOff == true)){
                //Altitude Controller. This is used either in Attitude Control Mode or while taking off.
                alt_error_pos = target_alt - position.y;

                float k_alt_pos = 2712f; 
                float z_alt_pos = .999263f;
                float p_alt_pos = .9937f;

                alt_output = k_alt_pos * alt_error_pos - k_alt_pos * z_alt_pos * prev_alt_error_pos + p_alt_pos * prev_alt_output_pos; //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
                if(alt_output > 1000){
                    alt_output = 1000;
                }
                else if(alt_output < -100){
                    alt_output = -100;
                }
                prev_alt_error_pos = alt_error_pos;
                prev_alt_output_pos = alt_output;

                if(takeoff_srv != null && takeoff_srv.takingOff == true){ //Checks if done taking off
                    if(position.y > takeoff_srv.takeoffAlt - takeoff_srv.takeoffError){
                        takeoff_srv.takingOff = false;
                    }
                }
            }

            else{
                //Vertical Speed controller 
                alt_error_vel = commandVelocityFLU.z - FLUVel.z;

                float k_alt_vel = 3000f;
                float z_alt_vel = .95f;
                float p_alt_vel = .9f;

                alt_output = k_alt_vel * alt_error_vel - k_alt_vel * z_alt_vel * prev_alt_error_vel + p_alt_vel * prev_alt_output_vel; //Using a lead/lag controller with the defined gain, k, zero, z, and pole, p.
                if(alt_output > 1000){
                    alt_output = 1000;
                }
                else if(alt_output < -1000){
                    alt_output = -1000;
                }
                prev_alt_error_vel = alt_error_vel;
                prev_alt_output_vel = alt_output;
            }

            var FL_RPM = base_speed + alt_output + pitch_output - roll_output - yaw_output;
            var FR_RPM = base_speed + alt_output + pitch_output + roll_output + yaw_output;
            var BL_RPM = base_speed + alt_output - pitch_output - roll_output + yaw_output;
            var BR_RPM = base_speed + alt_output - pitch_output + roll_output - yaw_output;
            
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