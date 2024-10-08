using System.Collections.Generic;
using Force;
using UnityEngine;
using Propeller = VehicleComponents.Actuators.Propeller;

namespace GameUI
{
    public class DroneKeyboardController : KeyboardController
    {

        public GameObject frontleftPropGo;
        public GameObject frontrightPropGo;
        public GameObject backrightPropGo;
        public GameObject backleftPropGo;

        Propeller frontrightProp, frontleftProp, backrightProp, backleftProp;

        [Tooltip("RPM to add to props when pressing IJKL")]
        public float MotionRPM = 1500f;
        [Tooltip("Extra RPMs to add when pressing space and IJKL")]
        public float LiftingRPM = 1500f;

        bool mouseDown = false;


        void Awake()
        {
            frontleftProp = frontleftPropGo.GetComponent<Propeller>();
            frontrightProp = frontrightPropGo.GetComponent<Propeller>();
            backrightProp = backrightPropGo.GetComponent<Propeller>();
            backleftProp = backleftPropGo.GetComponent<Propeller>();
        }

        void FixedUpdate()
        {
            if(Input.GetMouseButtonDown(1)) mouseDown = true;
            if(Input.GetMouseButtonUp(1)) mouseDown = false;
            if(mouseDown) return;

            var additionalRPM = MotionRPM;
            var backright = backrightProp.DefaultHoverRPM;
            var backleft = backleftProp.DefaultHoverRPM;
            var frontright = frontrightProp.DefaultHoverRPM;
            var frontleft = frontleftProp.DefaultHoverRPM;

            if(Input.GetKey(KeyCode.Space)) additionalRPM += LiftingRPM;

            if (Input.GetKey("i"))
            {
                backright += additionalRPM;
                backleft += additionalRPM;
                frontright -= additionalRPM;
                frontleft -= additionalRPM;
            }

            if (Input.GetKey("k"))
            {
                frontright += additionalRPM;
                frontleft += additionalRPM;
                backright -= additionalRPM;
                backleft -= additionalRPM;
            }

            if (Input.GetKey("j"))
            {
                frontright += additionalRPM;
                backright += additionalRPM;
                frontleft -= additionalRPM;
                backleft -= additionalRPM;
            }

            if (Input.GetKey("l"))
            {
                frontleft += additionalRPM;
                backleft += additionalRPM;
                frontright -= additionalRPM;
                backright -= additionalRPM;
            }

            if (Input.GetKey("u"))
            {
                frontleft += additionalRPM/4;
                frontright += additionalRPM/4;
                backleft += additionalRPM/4;
                backright += additionalRPM/4;
            }            

            if (Input.GetKey("n"))
            {
                frontleft -= additionalRPM/4;
                frontright -= additionalRPM/4;
                backleft -= additionalRPM/4;
                backright -= additionalRPM/4; 
            }
            
            backrightProp.SetRpm(backright);
            backleftProp.SetRpm(backleft);
            frontrightProp.SetRpm(frontright);
            frontleftProp.SetRpm(frontleft);

        }
    }
}