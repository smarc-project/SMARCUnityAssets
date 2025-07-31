using UnityEngine;
using VehicleComponents.Actuators;

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

        private float comPitch, comRoll;
        private Quaternion worldRotation;
        public Vector3 position;
        private Vector3 comVel;
        private Vector3 localVel;
        public Vector3 FLUVel;
        public float base_speed = 2900;
        public float target_alt;
        public float command_pitch = 0;
        public float command_roll = 0;
        public float target_pitch;
        public float target_roll;
        private float prev_alt_error = 0;
        public float prev_alt_output = 0;
        public float alt_error;
        private float prev_pitch_error = 0;
        public float prev_pitch_output = 0;
        public float pitch_error;

        private float vel_k_forw =  4f;
        private float vel_z_forw = .85f;
        private float vel_p_forw = .98f;

        private float vel_k_left =  40f;
        private float vel_z_left = .5f;
        private float vel_p_left = .985f;

        public Vector3 prev_vel_error = Vector3.zero;
        public Vector3 prev_vel_output = Vector3.zero;
        public Vector3 vel_output = Vector3.zero;
        public Vector3 vel_error;

        private float prev_roll_error = 0;
        public float prev_roll_output = 0;
        public float roll_error;

        void Awake(){
            FL.HoverDefault = false;
            BL.HoverDefault = false;
            FR.HoverDefault = false;
            BR.HoverDefault = false;
        }

        void FixedUpdate()
        {


            comPitch = ComAB.transform.rotation.eulerAngles.z;
            comRoll = ComAB.transform.rotation.eulerAngles.x;
            worldRotation = ComAB.transform.rotation;
            comVel = ComAB.linearVelocity;
            localVel = Quaternion.Inverse(worldRotation) * comVel;
            FLUVel.y = localVel.z;
            FLUVel.x = localVel.x;
            FLUVel.z = localVel.y;
            position = ComAB.transform.position;
            vel_error = Vector3.zero;
            if(controllerType == ControllerType.FLU_Velocity){
                vel_error = FLUVel - commandVelocityFLU;
            }
            if(controllerType == ControllerType.FLU_Attitude){
                target_pitch = command_pitch;
                target_roll = command_roll;
                vel_output = Vector3.zero;
            }
            else{
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

            pitch_error = target_pitch - comPitch;
            if (pitch_error > 180f) pitch_error -= 360f;
            if (pitch_error < -180f) pitch_error += 360f;

            //Pitch Controller Things
            float k = 30f; //Larger makes faster
            float z = .98f; //Larger makes mores stable (between 0 and 1)
            float p = .96f; //Larger makes "more aggressive", faster with more overshoot (between 0 and 1)

            var pitch_output = k * pitch_error - k * z * prev_pitch_error + p * prev_pitch_output;
            prev_pitch_error = pitch_error;
            prev_pitch_output = pitch_output;

            roll_error = target_roll - comRoll;
            if (roll_error > 180f) roll_error -= 360f;
            if (roll_error < -180f) roll_error += 360f;

            //Roll Controller Things
            k = 22f;
            z = .9985f;
            p = .97f;

            var roll_output = k * roll_error - k * z * prev_roll_error + p * prev_roll_output;
            prev_roll_error = roll_error;
            prev_roll_output = roll_output;

            alt_error = target_alt - position.y;

            var alt_output = 6590f * alt_error - 6508f * prev_alt_error + .9058f * prev_alt_output;

            prev_alt_error = alt_error;
            prev_alt_output = alt_output;
            
            FL.SetRpm(base_speed + alt_output + pitch_output - roll_output);
            FR.SetRpm(base_speed + alt_output + pitch_output + roll_output);
            BL.SetRpm(base_speed + alt_output - pitch_output - roll_output);
            BR.SetRpm(base_speed + alt_output - pitch_output + roll_output);
        }
        
    }
}