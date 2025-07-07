using UnityEngine;
using VehicleComponents.Actuators;

namespace dji
{
    public enum ControllerType
    {
        FLU_Velocity,
        ENU_RelativePosition,
    }

    public class DJIController : MonoBehaviour
    {
        public ArticulationBody ComAB;
        public ControllerType controllerType = ControllerType.FLU_Velocity;
        public Vector3 commandVelocityFLU = Vector3.zero;
        public Vector3 commandPositionENU = Vector3.zero;

        public Propeller FL, FR, BL, BR;

        private float comPitch, comRoll;
        private Vector3 comVel;
        public float kp;

        void FixedUpdate()
        {
            comPitch = ComAB.transform.rotation.eulerAngles.z;
            comRoll = ComAB.transform.rotation.eulerAngles.x;
            comVel = ComAB.linearVelocity;


            // super stupid example controller
            var pitchErr = comPitch;
            if (pitchErr > 180f) pitchErr -= 360f;
            if (pitchErr < -180f) pitchErr += 360f;

            FL.SetRpm(-pitchErr * kp);
            FR.SetRpm(-pitchErr * kp);


        }

    }
}