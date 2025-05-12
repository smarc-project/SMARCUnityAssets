using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


namespace SmarcGUI.MissionPlanning.Params
{
    public class OrientationParamGUI : ParamGUI, IParamHasOrientation
    {
        [Header("OrientationParamGUI")]
        public TMP_InputField exField;
        public TMP_InputField eyField, ezField;
        // Aldo said we dont need to show the quaternion to users. Thank you Aldo.


        public Orientation orientation
        {
            get { return (Orientation)paramValue; }
            set
            {
                var oriParam = (Orientation)paramValue;
                oriParam.x = value.x;
                oriParam.y = value.y;
                oriParam.z = value.z;
                oriParam.w = value.w;
                paramValue = oriParam;
                var euler = oriParam.ToRPY();
                exField.text = euler.x.ToString();
                eyField.text = euler.y.ToString();
                ezField.text = euler.z.ToString();
                NotifyPathChange();
            }
        }


        void UpdateOrientationFromEuler()
        {
            var ex = exField.text != "" ? float.Parse(exField.text) : 0;
            var ey = eyField.text != "" ? float.Parse(eyField.text) : 0;
            var ez = ezField.text != "" ? float.Parse(ezField.text) : 0;

            orientation = new Orientation(ex, ey, ez);
        }


        void Awake()
        {
            guiState = FindFirstObjectByType<GUIState>();
        }

        protected override void SetupFields()
        {
            var rosOri = (Orientation)paramValue;
            if(rosOri.x == 0 && rosOri.y == 0 && rosOri.z == 0 && rosOri.w == 0)
            {
                rosOri.x = 0;
                rosOri.y = 0;
                rosOri.z = 0;
                rosOri.w = 1;
            }
            paramValue = rosOri;

            var euler = rosOri.ToRPY();
            exField.text = euler.x.ToString();
            eyField.text = euler.y.ToString();
            ezField.text = euler.z.ToString();

            exField.onEndEdit.AddListener(OnEulerXChanged);
            eyField.onEndEdit.AddListener(OnEulerYChanged);
            ezField.onEndEdit.AddListener(OnEulerZChanged);

            fields.Add(exField.GetComponent<RectTransform>());
            fields.Add(eyField.GetComponent<RectTransform>());
            fields.Add(ezField.GetComponent<RectTransform>());

            OnSelectedChange();
        }

        public override List<string> GetFieldLabels()
        {
            return new List<string> { "Roll", "Pitch", "Yaw" };
        }



        void OnEulerXChanged(string s)
        {
            try {UpdateOrientationFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler X value");
                OnEulerXChanged("0");
                return;
            }
        }

        void OnEulerYChanged(string s)
        {
            try {UpdateOrientationFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler Y value");
                OnEulerYChanged("0");
                return;
            }
        }

        void OnEulerZChanged(string s)
        {
            try {UpdateOrientationFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler Z value");
                OnEulerZChanged("0");
                return;
            }
        }

        public Orientation GetROSOrientation()
        {
            return (Orientation)paramValue;
        }

        public void SetROSOrientation(Orientation orientation)
        {
            paramValue = orientation;
        }

        public Quaternion GetUnityQuaternion()
        {
            var o = (Orientation)paramValue;
            var unityOri = ENU.ConvertToRUF(
                        new Quaternion(
                            (float)o.x,
                            (float)o.y,
                            (float)o.z,
                            (float)o.w));

            return unityOri;
        }

        public void SetUnityQuaternion(Quaternion q)
        {
            // might be nice to have later, when there is a gui widget for setting the orientation
            // in-world
            Debug.Log("SetUnityQuat not implemented");
        }
    }
}
