using UnityEngine;
using UnityEngine.InputSystem;

using Hinge = VehicleComponents.Actuators.Hinge;
using Propeller = VehicleComponents.Actuators.Propeller;

namespace SmarcGUI.KeyboardControllers
{
    public class LOLOKeyboardControl : KeyboardControllerBase
    {

        public GameObject elevatorHingeGo, elevonPortHingeGo, elevonStbdHingeGo;
        public GameObject rudderPortHingeGo, rudderStbdHingeGo;
        public GameObject thrusterPortGo, thrusterStbdGo;
        public GameObject verticalThrusterFrontPortGo, verticalThrusterFrontStbdGo;
        public GameObject verticalThrusterBackPortGo, verticalThrusterBackStbdGo;

        

        Hinge elevatorHinge, elevonPortHinge, elevonStbdHinge;
        Hinge rudderPortHinge, rudderStbdHinge;
        Propeller thrusterPort, thrusterStbd;
        Propeller verticalThrusterFrontPort, verticalThrusterFrontStbd;
        Propeller verticalThrusterBackPort, verticalThrusterBackStbd;
       
        public float moveRpms = 200f;
        public float verticalRpms = 2000f;


        InputAction forwardAction, verticalAction, tvAction, rollAction, pitchAction;
        

        void Awake()
        {
            elevatorHinge = elevatorHingeGo.GetComponent<Hinge>();
            elevonPortHinge = elevonPortHingeGo.GetComponent<Hinge>();
            elevonStbdHinge = elevonStbdHingeGo.GetComponent<Hinge>();
            rudderPortHinge = rudderPortHingeGo.GetComponent<Hinge>();
            rudderStbdHinge = rudderStbdHingeGo.GetComponent<Hinge>();
            thrusterPort = thrusterPortGo.GetComponent<Propeller>();
            thrusterStbd = thrusterStbdGo.GetComponent<Propeller>();
            verticalThrusterFrontPort = verticalThrusterFrontPortGo.GetComponent<Propeller>();
            verticalThrusterFrontStbd = verticalThrusterFrontStbdGo.GetComponent<Propeller>();
            verticalThrusterBackPort = verticalThrusterBackPortGo.GetComponent<Propeller>();
            verticalThrusterBackStbd = verticalThrusterBackStbdGo.GetComponent<Propeller>();
            
            forwardAction = InputSystem.actions.FindAction("Robot/Forward");
            verticalAction = InputSystem.actions.FindAction("Robot/UpDown");
            tvAction = InputSystem.actions.FindAction("Robot/ThrustVector");
            rollAction = InputSystem.actions.FindAction("Robot/Roll");
            pitchAction = InputSystem.actions.FindAction("Robot/Pitch");

        }

        void Update()
        {
            var forwardValue = forwardAction.ReadValue<float>();
            var verticalValue = verticalAction.ReadValue<float>();
            var rollValue = rollAction.ReadValue<float>();
            var pitchValue = pitchAction.ReadValue<float>();
            var tv = tvAction.ReadValue<Vector2>();
            var rudderValue = tv.x;
            var elevatorValue = tv.y;

            thrusterPort.SetRpm((forwardValue + rudderValue) * moveRpms);
            thrusterStbd.SetRpm((forwardValue - rudderValue) * moveRpms);
            
            rudderPortHinge.SetAngle(rudderValue * rudderPortHinge.AngleMax);
            rudderStbdHinge.SetAngle(rudderValue * rudderStbdHinge.AngleMax);

            elevatorHinge.SetAngle((elevatorValue + pitchValue) * elevatorHinge.AngleMax);

            elevonPortHinge.SetAngle((elevatorValue + rollValue) * elevonPortHinge.AngleMax);
            elevonStbdHinge.SetAngle((elevatorValue - rollValue) * elevonStbdHinge.AngleMax);

            verticalThrusterFrontPort.SetRpm((verticalValue + rollValue + pitchValue) * verticalRpms);
            verticalThrusterFrontStbd.SetRpm((verticalValue - rollValue + pitchValue) * verticalRpms);
            verticalThrusterBackPort.SetRpm((verticalValue + rollValue - pitchValue) * verticalRpms);
            verticalThrusterBackStbd.SetRpm((verticalValue - rollValue - pitchValue) * verticalRpms);
        }

        public override void OnReset()
        {
            elevatorHinge.SetAngle(0f);
            elevonPortHinge.SetAngle(0f);
            elevonStbdHinge.SetAngle(0f);
            rudderPortHinge.SetAngle(0f);
            rudderStbdHinge.SetAngle(0f);
            thrusterPort.SetRpm(0f);
            thrusterStbd.SetRpm(0f);
            verticalThrusterFrontPort.SetRpm(0f);
            verticalThrusterFrontStbd.SetRpm(0f);
            verticalThrusterBackPort.SetRpm(0f);
            verticalThrusterBackStbd.SetRpm(0f);
        }

    }
}