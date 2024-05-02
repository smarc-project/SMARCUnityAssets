using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(ISAMControl))]
    [RequireComponent(typeof(SamActuatorController))]
    public class SAMKeyboardControl : MonoBehaviour
    {
        private ISAMControl _samControl;

        private SamActuatorController rosControl;
        [Tooltip("If true, pressing any key will disable the ros controllers")]
        public bool takeOverRosController = true;

        private List<ForcePoint> points;
        public float angles = 0.1f;
        public float rollRpms = 100f;
        public float moveRpms = 800f;

        [Header("Mouse control")]
        [Tooltip("Use these when you dont want to press down for 10 minutes")]
        public bool useBothRpms = false;
        public float bothRpms = 0f;


        private void Awake()
        {
            _samControl = GetComponent<ISAMControl>();
            rosControl = GetComponent<SamActuatorController>();
            points = new List<ForcePoint>(GetComponentsInChildren<ForcePoint>());

        }

        private void Update()
        {
            if(useBothRpms)
            {
                _samControl.SetRpm(bothRpms, bothRpms);
            }

            if (Input.GetKeyDown("down"))
            {
                _samControl.SetRpm(-moveRpms, -moveRpms);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("q"))
            {
                _samControl.SetRpm(-rollRpms, rollRpms);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("e"))
            {
                _samControl.SetRpm(rollRpms, -rollRpms);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("up"))
            {
                _samControl.SetRpm(moveRpms, moveRpms);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyUp("up") || Input.GetKeyUp("down") || Input.GetKeyUp("q") || Input.GetKeyUp("e"))
            {
                _samControl.SetRpm(0, 0);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("a"))
            {
                _samControl.SetRudderAngle(-angles);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("d"))
            {
                _samControl.SetRudderAngle(angles);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
            {
                _samControl.SetRudderAngle(0);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("w"))
            {
                _samControl.SetElevatorAngle(-angles);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("s"))
            {
                _samControl.SetElevatorAngle(angles);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyUp("w") || Input.GetKeyUp("s"))
            {
                _samControl.SetElevatorAngle(0);
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("c"))
            {
                points.ForEach(point => point.displacementAmount = 1.0f );
                if(takeOverRosController) rosControl.enable=false;
            }

            if (Input.GetKeyDown("space"))
            {
                points.ForEach(point => point.displacementAmount = 1.1f );
                if(takeOverRosController) rosControl.enable=false;
            }

        }


    }

}
