using UnityEngine;
using UnityEngine.InputSystem;
using Propeller = VehicleComponents.Actuators.Propeller;

namespace SmarcGUI.KeyboardControllers
{
    public class DroneKeyboardController : KeyboardControllerBase
    {

        public GameObject frontleftPropGo;
        public GameObject frontrightPropGo;
        public GameObject backrightPropGo;
        public GameObject backleftPropGo;

        Propeller frontrightProp, frontleftProp, backrightProp, backleftProp;

        [Tooltip("RPM to add to props when pressing IJKL")]
        public float MotionRPM = 1500f;

        InputAction forwardAction, strafeAction, verticalAction, pitchAction, rollAction;

        void Awake()
        {
            frontleftProp = frontleftPropGo.GetComponent<Propeller>();
            frontrightProp = frontrightPropGo.GetComponent<Propeller>();
            backrightProp = backrightPropGo.GetComponent<Propeller>();
            backleftProp = backleftPropGo.GetComponent<Propeller>();

            forwardAction = InputSystem.actions.FindAction("Robot/Forward");
            strafeAction = InputSystem.actions.FindAction("Robot/Strafe");
            verticalAction = InputSystem.actions.FindAction("Robot/UpDown");
            pitchAction = InputSystem.actions.FindAction("Robot/Pitch");
            rollAction = InputSystem.actions.FindAction("Robot/Roll");
        }

        void Update()
        {
            var forwardValue = forwardAction.ReadValue<float>();
            var strafeValue = strafeAction.ReadValue<float>();
            var verticalValue = verticalAction.ReadValue<float>();
            var pitchValue = pitchAction.ReadValue<float>();
            var rollValue = rollAction.ReadValue<float>();

            var FR = frontrightProp.DefaultHoverRPM;
            var FL = frontleftProp.DefaultHoverRPM;
            var BR = backrightProp.DefaultHoverRPM;
            var BL = backleftProp.DefaultHoverRPM;

            BR += (forwardValue+pitchValue) * MotionRPM;
            BL += (forwardValue+pitchValue) * MotionRPM;
            FR -= (forwardValue+pitchValue) * MotionRPM;
            FL -= (forwardValue+pitchValue) * MotionRPM;

            BL += (rollValue + strafeValue) * MotionRPM;
            FL += (rollValue + strafeValue) * MotionRPM;
            FR -= (rollValue + strafeValue) * MotionRPM;
            BR -= (rollValue + strafeValue) * MotionRPM;

            BR += verticalValue * MotionRPM;
            BL += verticalValue * MotionRPM;
            FR += verticalValue * MotionRPM;
            FL += verticalValue * MotionRPM;

            frontrightProp.SetRpm(FR);
            frontleftProp.SetRpm(FL);
            backrightProp.SetRpm(BR);
            backleftProp.SetRpm(BL);
        }   

        void AdjustRPM(Propeller prop, float adjustment)
        {
            prop.SetRpm(prop.DefaultHoverRPM + adjustment);
        }

        public override void OnReset()
        {
            AdjustRPM(backrightProp, 0);
            AdjustRPM(backleftProp, 0);
            AdjustRPM(frontrightProp, 0);
            AdjustRPM(frontleftProp, 0);
        }

    }
}