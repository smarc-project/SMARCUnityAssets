using UnityEngine;
using UnityEngine.InputSystem;
using Propeller = VehicleComponents.Actuators.Propeller;

namespace SmarcGUI.KeyboardControllers
{
    public class DroneKeyboardController : KeyboardControllerBase
    {
        [Tooltip("The keyboard controller will just set the target of the drone and let the drone controller do the rest")]
        public Transform DroneTarget;
        public Transform DroneBaseLink;

        InputAction forwardAction, strafeAction, verticalAction, pitchAction, rollAction;

        void Awake()
        {
            forwardAction = InputSystem.actions.FindAction("Robot/Forward");
            strafeAction = InputSystem.actions.FindAction("Robot/Strafe");
            verticalAction = InputSystem.actions.FindAction("Robot/UpDown");
        }

        void Update()
        {
            if (DroneTarget == null || DroneBaseLink == null)
            {
                Debug.LogWarning("DroneTarget or DroneBaseLink is not set. Please assign them in the inspector. Disabling the script.");
                gameObject.SetActive(false);
                return;
            }

            var forwardValue = forwardAction.ReadValue<float>();
            var strafeValue = strafeAction.ReadValue<float>();
            var verticalValue = verticalAction.ReadValue<float>();
            
            float d = 1;
            Vector3 motion = new Vector3(forwardValue, verticalValue, strafeValue) * d;
            DroneTarget.position = DroneBaseLink.position + motion;
        }   

        public override void OnReset()
        {
            DroneTarget.position = DroneBaseLink.position;
        }

    }
}