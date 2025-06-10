using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


namespace SmarcGUI.MissionPlanning.Params
{    public class AuvHydrobaticPointGUI : ParamGUI, IParamHasXZ, IParamHasY, IParamHasOrientation, IParamHasTolerance
    {
        [Header("AuvHydrobaticPointGUI")]
        public TMP_InputField LatField;
        public TMP_InputField LonField, TargetDepthField, TimeoutField, ToleranceField;
        public TMP_InputField exField, eyField, ezField;

        public double latitude
        {
            get { return ((AuvHydrobaticPoint)paramValue).latitude; }
            set
            {
                var gp = (AuvHydrobaticPoint)paramValue;
                gp.latitude = value;
                paramValue = gp;
                LatField.text = value.ToString();
                NotifyPathChange();
            }
        }
        public double longitude
        {
            get { return ((AuvHydrobaticPoint)paramValue).longitude; }
            set
            {
                var gp = (AuvHydrobaticPoint)paramValue;
                gp.longitude = value;
                paramValue = gp;
                LonField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float target_depth
        {
            get { return ((AuvHydrobaticPoint)paramValue).target_depth; }
            set
            {
                var d = (AuvHydrobaticPoint)paramValue;
                d.target_depth = value;
                paramValue = d;
                TargetDepthField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float timeout
        {
            get { return ((AuvHydrobaticPoint)paramValue).timeout; }
            set
            {
                var d = (AuvHydrobaticPoint)paramValue;
                d.timeout = value;
                paramValue = d;
                TimeoutField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public Orientation orientation
        {
            get { return ((AuvHydrobaticPoint)paramValue).orientation; }
            set
            {
                var oriParam = (AuvHydrobaticPoint)paramValue;
                oriParam.orientation = value;
                paramValue = oriParam;
                var euler = value.ToRPY();
                exField.text = (Mathf.Abs(euler.x) < 0.0001f ? 0 : euler.x).ToString();
                eyField.text = (Mathf.Abs(euler.y) < 0.0001f ? 0 : euler.y).ToString();
                ezField.text = (Mathf.Abs(euler.z) < 0.0001f ? 0 : euler.z).ToString();
                NotifyPathChange();
            }
        }

        public float tolerance
        {
            get { return ((AuvHydrobaticPoint)paramValue).tolerance; }
            set
            {
                var d = (AuvHydrobaticPoint)paramValue;
                d.tolerance = value;
                paramValue = d;
                ToleranceField.text = value.ToString();
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



        protected override void SetupFields()
        {
            if (latitude == 0 && longitude == 0)
            {
                // set this to be the same as the previous geo point
                if (ParamIndex > 0)
                {
                    var previousGp = (AuvHydrobaticPoint)paramsList[ParamIndex - 1];
                    latitude = previousGp.latitude;
                    longitude = previousGp.longitude;
                    target_depth = previousGp.target_depth;
                    timeout = previousGp.timeout;
                    orientation = previousGp.orientation;
                    tolerance = previousGp.tolerance;
                    guiState.Log("New LatLon set to previous.");
                }
                // if there is no previous geo point, set it to where the camera is looking at
                else
                {
                    var point = guiState.GetLookAtPoint();
                    var (lat, lon) = GetLatLonFromUnityXZ(point.x, point.z);
                    latitude = lat;
                    longitude = lon;
                    guiState.Log("New LatLon set to where the camera is looking at.");
                }
            }

            if (target_depth == 0) target_depth = -1;
            if (tolerance == 0) tolerance = 1;

            LatField.text = latitude.ToString();
            LonField.text = longitude.ToString();
            TargetDepthField.text = target_depth.ToString();
            TimeoutField.text = timeout.ToString();
            ToleranceField.text = tolerance.ToString();
            var euler = orientation.ToRPY();
            exField.text = euler.x.ToString();
            eyField.text = euler.y.ToString();
            ezField.text = euler.z.ToString();

            OnEulerXChanged("0"); // just trigger a change to set the orientation of the widget?

            LatField.onEndEdit.AddListener(OnLatChanged);
            LonField.onEndEdit.AddListener(OnLonChanged);
            TargetDepthField.onEndEdit.AddListener(OnDepthChanged);
            TimeoutField.onEndEdit.AddListener(OnTimeoutChanged);
            ToleranceField.onEndEdit.AddListener(OnToleranceChanged);
            exField.onEndEdit.AddListener(OnEulerXChanged);
            eyField.onEndEdit.AddListener(OnEulerYChanged);
            ezField.onEndEdit.AddListener(OnEulerZChanged);


            fields.Add(LatField.GetComponent<RectTransform>());
            fields.Add(LonField.GetComponent<RectTransform>());
            fields.Add(TargetDepthField.GetComponent<RectTransform>());
            fields.Add(TimeoutField.GetComponent<RectTransform>());
            fields.Add(exField.GetComponent<RectTransform>());
            fields.Add(eyField.GetComponent<RectTransform>());
            fields.Add(ezField.GetComponent<RectTransform>());
            fields.Add(ToleranceField.GetComponent<RectTransform>());

            OnSelectedChange();
        }

        public override List<string> GetFieldLabels()
        {
            return new List<string> { "Lat", "Lon", "T.Depth", "T/O", "Roll", "Pitch", "Yaw", "Tol"  };
        }

        void OnToleranceChanged(string s)
        {
            try { tolerance = float.Parse(s); }
            catch
            {
                guiState.Log("Invalid tolerance value");
                OnToleranceChanged(tolerance.ToString());
                return;
            }
            NotifyPathChange();
        }

        void OnEulerXChanged(string s)
        {
            try { UpdateOrientationFromEuler(); }
            catch
            {
                guiState.Log("Invalid euler X value");
                OnEulerXChanged("0");
                return;
            }
            NotifyPathChange();
        }

        void OnEulerYChanged(string s)
        {
            try { UpdateOrientationFromEuler(); }
            catch
            {
                guiState.Log("Invalid euler Y value");
                OnEulerYChanged("0");
                return;
            }
            NotifyPathChange();
        }

        void OnEulerZChanged(string s)
        {
            try { UpdateOrientationFromEuler(); }
            catch
            {
                guiState.Log("Invalid euler Z value");
                OnEulerZChanged("0");
                return;
            }
            NotifyPathChange();
        }


        void OnLatChanged(string s)
        {
            try { latitude = double.Parse(s); }
            catch
            {
                guiState.Log("Invalid latitude value");
                OnLatChanged(latitude.ToString());
                return;
            }
            NotifyPathChange();
        }

        void OnLonChanged(string s)
        {
            try { longitude = double.Parse(s); }
            catch
            {
                guiState.Log("Invalid longitude value");
                OnLonChanged(longitude.ToString());
                return;
            }
            NotifyPathChange();
        }

        void OnDepthChanged(string s)
        {
            try { target_depth = float.Parse(s); }
            catch
            {
                guiState.Log("Invalid depth value");
                OnDepthChanged(target_depth.ToString());
                return;
            }
            NotifyPathChange();
        }


        void OnTimeoutChanged(string s)
        {
            try { timeout = float.Parse(s); }
            catch
            {
                guiState.Log("Invalid timeout value");
                OnTimeoutChanged(timeout.ToString());
                return;
            }
            NotifyPathChange();
        }



        public (float, float) GetXZ()
        {
            var (tx, tz) = GetUnityXZFromLatLon(latitude, longitude);
            return ((float)tx, (float)tz);
        }

        public void SetXZ(float x, float z)
        {
            var (lat, lon) = GetLatLonFromUnityXZ(x, z);
            latitude = lat;
            longitude = lon;
        }

        public float GetY()
        {
            return -target_depth;
        }

        public float GetYReference()
        {
            return 0;
        }

        public void SetY(float y)
        {
            target_depth = -y;
        }

        public Orientation GetROSOrientation()
        {
            return orientation;
        }

        public void SetROSOrientation(Orientation orientation)
        {
            this.orientation = orientation;
        }

        public Quaternion GetUnityQuaternion()
        {
            var unityOri = ENU.ConvertToRUF(
                        new Quaternion(
                            (float)orientation.x,
                            (float)orientation.y,
                            (float)orientation.z,
                            (float)orientation.w));

            return unityOri;
        }

        public void SetUnityQuaternion(Quaternion q)
        {
            // might be nice to have later, when there is a gui widget for setting the orientation
            // in-world
            Debug.Log("SetUnityQuat not implemented");
        }
        
        public float GetTolerance()
        {
            return tolerance;
        }

        public void SetTolerance(float y)
        {
            tolerance = y;
        }

    }
}
