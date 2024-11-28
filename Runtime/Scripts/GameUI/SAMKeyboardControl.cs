using System.Collections.Generic;
using Force;
using UnityEngine;
using Hinge = VehicleComponents.Actuators.Hinge;
using Propeller = VehicleComponents.Actuators.Propeller;
using VBS = VehicleComponents.Actuators.VBS;
using Prismatic = VehicleComponents.Actuators.Prismatic;

namespace GameUI
{
    public class SAMKeyboardControl : KeyboardController
    {

        public GameObject yawHingeGo;
        public GameObject pitchHingeGo;
        public GameObject frontPropGo;
        public GameObject backPropGo;
        public GameObject vbsGo;
        public GameObject lcgGo;

        Hinge yaw, pitch;
        Propeller frontProp, backProp;
        VBS vbs;
        Prismatic lcg;


        bool mouseDown = false;


        public float rollRpms = 0.1f;
        public float moveRpms = 800f;

        [Header("Bricks on Keys")] [Tooltip("Use these when you dont want to press down for 10 minutes")]
        public List<string> PutABrickOnKeys = new List<string>();

        bool GetKeyDown(string key)
        {
            if (PutABrickOnKeys.Contains(key))
            {
                return true;
            }
            return Input.GetKeyDown(key);
        }

        void Awake()
        {
            yaw = yawHingeGo.GetComponent<Hinge>();
            pitch = pitchHingeGo.GetComponent<Hinge>();
            frontProp = frontPropGo.GetComponent<Propeller>();
            backProp = backPropGo.GetComponent<Propeller>();
            vbs = vbsGo.GetComponent<VBS>();
            lcg = lcgGo.GetComponent<Prismatic>();
        }

        void Update()
        {

            // Ignore inputs while the right mouse
            // button is held down. Since this is used for camera controls.
            // There is no "while button down" check, so we DIY.
            if(Input.GetMouseButtonDown(1)) mouseDown = true;
            if(Input.GetMouseButtonUp(1)) mouseDown = false;
            if(mouseDown) return;

            if (GetKeyDown("down"))
            {
                frontProp.SetRpm(-moveRpms);
                backProp.SetRpm(-moveRpms);
            }

            if (GetKeyDown("q"))
            {
                frontProp.SetRpm(-rollRpms);
                backProp.SetRpm(rollRpms);
            }

            if (GetKeyDown("e"))
            {
                frontProp.SetRpm(rollRpms);
                backProp.SetRpm(-rollRpms);
            }

            if (GetKeyDown("up"))
            {
                frontProp.SetRpm(moveRpms);
                backProp.SetRpm(moveRpms);
            }

            if (Input.GetKeyUp("up") || Input.GetKeyUp("down") || Input.GetKeyUp("q") || Input.GetKeyUp("e"))
            {
                frontProp.SetRpm(0);
                backProp.SetRpm(0);
            }

            if (GetKeyDown("a"))
            {
                yaw.SetAngle(-1);
            }

            if (GetKeyDown("d"))
            {
                yaw.SetAngle(1);
            }

            if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
            {
                yaw.SetAngle(0);
            }

            if (GetKeyDown("w"))
            {
                pitch.SetAngle(-1);
            }

            if (GetKeyDown("s"))
            {
                pitch.SetAngle(1);
            }

            if (Input.GetKeyUp("w") || Input.GetKeyUp("s"))
            {
                pitch.SetAngle(0);
            }

            if (GetKeyDown("r"))
            {
                vbs.SetPercentage(0f);
            }

            if (GetKeyDown("f"))
            {
                vbs.SetPercentage(50f);
            }

            if (GetKeyDown("c"))
            {
                vbs.SetPercentage(100f);
            }

            if (GetKeyDown("t"))
            {
                lcg.SetPercentage(0f);
            }

            if (GetKeyDown("g"))
            {
                lcg.SetPercentage(50f);
            }

            if (GetKeyDown("v"))
            {
                lcg.SetPercentage(100f);
            }
        }
    }
}