using UnityEngine;
using VehicleComponents.Actuators;
using System;
using System.IO;
using System.Globalization;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace dji
{
    
    public enum ControllerType
    {
        FLU_Velocity,
        ENU_RelativePosition,
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
        public float base_speed = 3000;
        public float target_alt;
        public float command_pitch = 0;
        public float command_roll = 0;
        public float command_yaw = 0;
        public float target_pitch;
        public float target_roll;
        public float target_yaw;
        private float prev_alt_error_pos = 0;
        private float prev_alt_output_pos = 0;
        private float alt_error_pos;
        public float prev_alt_error_vel = 0;
        public float prev_alt_output_vel = 0;
        private float alt_error_vel;
        private float prev_pitch_error = 0;
        private float prev_pitch_output = 0;
        private float pitch_error;


        private float vel_k_forw = 30f;
        private float vel_z_forw = .9f;
        private float vel_p_forw = .8f;

        public float vel_k_left =  30f;
        public float vel_z_left = 0f;
        public float vel_p_left = 0f;

        private Vector3 prev_vel_error = Vector3.zero;
        private Vector3 prev_vel_output = Vector3.zero;
        private Vector3 vel_output = Vector3.zero;
        private Vector3 vel_error;

        private float prev_roll_error = 0;
        private float prev_roll_output = 0;
        private float roll_error;

        private float prev_yaw_error = 0;
        private float prev_yaw_output = 0;
        public float yaw_error;
        public float yaw_output;

        public float counter = 0;
        private float counter_max = 25;

        public string data_path = @"c:\School\UnityLogger.csv";

        public string takeoff_service_name = "/m350_v1/wrapper/psdk_ros2/takeoff";
        public string robot_name = "M350/";
        void Awake(){
            FL.HoverDefault = false;
            BL.HoverDefault = false;
            FR.HoverDefault = false;
            BR.HoverDefault = false;
        }

        void Start(){
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
            ROSConnection ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<Float32Msg>(robot_name + "sim/target_alt", _target_alt_sub_callback); //TODO: Make variable
            ros.Subscribe<Vector3StampedMsg>(robot_name + "sim/joy", _joy_sub_callback); //TODO: Make variable
            ros.Subscribe<Int8Msg>(robot_name + "sim/control_mode", _control_sub_callback); //TODO: Make variable
        }
 
        void FixedUpdate()
        {
            counter++;
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

            target_yaw = command_yaw;

            if(controllerType == ControllerType.FLU_Velocity){
                vel_error = FLUVel - commandVelocityFLU;
            }
            if(controllerType == ControllerType.FLU_Attitude){
                target_pitch = command_pitch;
                target_roll = command_roll;
                vel_output = Vector3.zero;
            }
            else if(counter >= counter_max){
                counter = 0;
                target_pitch = vel_error.x * vel_k_forw - prev_vel_error.x * vel_k_forw * vel_z_forw + prev_vel_output.x * vel_p_forw;
                target_roll = -(vel_error.y * vel_k_left - prev_vel_error.y * vel_k_left * vel_z_left + prev_vel_output.y * vel_p_left);
                prev_vel_error = vel_error;
                
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
            if(counter >= counter_max){
                counter = 0;
            }

            pitch_error = target_pitch - comPitch;
            if (pitch_error > 180f) pitch_error -= 360f;
            if (pitch_error < -180f) pitch_error += 360f;

            //Pitch Controller Things. In testing, this gave a .05 s rise time and 75% overshoot.
            float k = 1200f; //Larger makes faster
            float z = 0.98f; //Larger makes mores stable (between 0 and 1)
            float p = .95f; //Larger makes "more aggressive", faster with more overshoot (between 0 and 1)

            var pitch_output = k * pitch_error - k * z * prev_pitch_error + p * prev_pitch_output;
            if(pitch_output > 1000){
                pitch_output = 1000;
            }
            else if(pitch_output < -1000){
                pitch_output = -1000;
            }
            prev_pitch_error = pitch_error;
            prev_pitch_output = pitch_output;

            roll_error = target_roll - comRoll;
            if (roll_error > 180f) roll_error -= 360f;
            if (roll_error < -180f) roll_error += 360f;

            //Roll Controller Things. In testing, this gave a .075 s rise time and 98% overshoot.
            k = 1200f;
            z = .98f;
            p = .95f;

            var roll_output = k * roll_error - k * z * prev_roll_error + p * prev_roll_output;
            if(roll_output > 1000){
                roll_output = 1000;
            }
            else if(roll_output < -1000){
                roll_output = -1000;
            }
            prev_roll_error = roll_error;
            prev_roll_output = roll_output;

            yaw_error = target_yaw - comYaw;
            if (yaw_error > 180f) yaw_error -= 360f;
            if (yaw_error < -180f) yaw_error += 360f;

            float k_yaw = 8f;
            float z_yaw = .98f;
            float p_yaw = .8f;

            yaw_output = k_yaw * yaw_error - k_yaw * z_yaw * prev_yaw_error + p_yaw * prev_yaw_output ;
            if(yaw_output > 100){
                yaw_output = 100;
            }
            else if(yaw_output < -100){
                yaw_output = -100;
            }
            prev_yaw_error = yaw_error;
            prev_yaw_output = yaw_output;

            float alt_output;
            
            if(controllerType == ControllerType.FLU_Attitude){
                alt_error_pos = target_alt - position.y;

                float k_alt_pos = 2712f;
                float z_alt_pos = .999263f;
                float p_alt_pos = .9937f;

                alt_output = k_alt_pos * alt_error_pos - k_alt_pos * z_alt_pos * prev_alt_error_pos + p_alt_pos * prev_alt_output_pos;
                if(alt_output > 1000){
                    alt_output = 1000;
                }
                else if(alt_output < -100){
                    alt_output = -100;
                }
                prev_alt_error_pos = alt_error_pos;
                prev_alt_output_pos = alt_output;
            }

            else{
                alt_error_vel = commandVelocityFLU.z - FLUVel.z;

                float k_alt_vel = 3000f;
                float z_alt_vel = .95f;
                float p_alt_vel = .9f;

                alt_output = k_alt_vel * alt_error_vel - k_alt_vel * z_alt_vel * prev_alt_error_vel + p_alt_vel * prev_alt_output_vel;
                if(alt_output > 1000){
                    alt_output = 1000;
                }
                else if(alt_output < -100){
                    alt_output = -100;
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
            using (StreamWriter sw = File.AppendText(data_path)){
                    sw.WriteLine($"{Time.fixedTime},{comRoll},{comPitch},{comYaw},{target_roll},{target_pitch},{target_yaw},{FLUVel.x},{FLUVel.y},{FLUVel.z},{commandVelocityFLU.x},{commandVelocityFLU.y},{commandVelocityFLU.z}, {FL_RPM}, {FR_RPM}, {BL_RPM}, {BR_RPM}, {target_pitch}, {target_roll}");
            }
        }

        void _target_alt_sub_callback(Float32Msg msg){
            target_alt = (float)msg.data;
        }

        void _joy_sub_callback(Vector3StampedMsg msg){
            commandVelocityFLU.x = (float)msg.vector.x;
            commandVelocityFLU.y = (float)msg.vector.y;
            commandVelocityFLU.z = (float)msg.vector.z;
        }
 
        void _control_sub_callback(Int8Msg msg){
            if(msg.data == 1){
                controllerType = ControllerType.FLU_Velocity;
            }
            else{
                controllerType = ControllerType.FLU_Attitude;
            }
        }
        
    }
}