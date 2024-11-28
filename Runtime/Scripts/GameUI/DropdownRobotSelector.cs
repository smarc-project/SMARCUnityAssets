using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using VehicleComponents.ROS.Subscribers;

namespace GameUI
{
    public class DropdownRobotSelector : MonoBehaviour
    {
        public GameObject DropdownRobotSelect;
        public GameObject ToggleROSControl;
        public GameObject ToggleKBControl;

        TMP_Dropdown dropdown;
        Toggle toggle_rosControl;
        Toggle toggle_kbControl;
        
        GameObject selectedRobotRoot;
        
        void Start()
        {
            dropdown = DropdownRobotSelect.GetComponent<TMP_Dropdown>();
            toggle_rosControl = ToggleROSControl.GetComponent<Toggle>();
            toggle_kbControl = ToggleKBControl.GetComponent<Toggle>();
            
            // Get all the #robot tagged objects in the scene
            // then we'll use their root name in the list
            var robots = GameObject.FindGameObjectsWithTag("robot");
            if(robots.Length <= 0) return;
            
            foreach(var robot in robots)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(){text=robot.transform.root.name});
            }

            selectedRobotRoot = robots[0].transform.root.gameObject;
            dropdown.value = 0;
            dropdown.RefreshShownValue();
            OnToggleKBControl(true);
            UpdateToggles();
        }

        void UpdateToggles()
        {
            bool atLeastOneROS = false;
            var actSubs = selectedRobotRoot.GetComponentsInChildren<ActuatorSubscriber>();
            foreach(var actSub in actSubs)
            {
                atLeastOneROS = atLeastOneROS || actSub.enabled;
            }
            toggle_rosControl.isOn = atLeastOneROS;

            var kbc = selectedRobotRoot.GetComponentInChildren<KeyboardController>();
            if(kbc != null)
            {
                toggle_kbControl.isOn = kbc.enabled && !atLeastOneROS;
            }
        }

        public void OnValueChanged(int ddIndex)
        {
            var selection = dropdown.options[ddIndex];
            selectedRobotRoot = GameObject.Find(selection.text);
            UpdateToggles();
        }

        public void OnToggleROSControl(bool t)
        {
            var actSubs = selectedRobotRoot.GetComponentsInChildren<ActuatorSubscriber>();
            foreach(var actSub in actSubs)
            {
                actSub.enabled = t;
                if(t)
                {
                    OnToggleKBControl(false);
                    UpdateToggles();
                }
            }
        }

        public void OnToggleKBControl(bool t)
        {
            KeyboardController kbc = selectedRobotRoot.GetComponentInChildren<KeyboardController>();
            if(kbc != null)
            {
                kbc.enabled = t;
                if(t)
                {
                    OnToggleROSControl(false);
                    UpdateToggles();
                }
            }
        }

    
}
}

