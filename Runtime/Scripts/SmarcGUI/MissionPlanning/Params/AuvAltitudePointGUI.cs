using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;
using System.Collections.Generic;


namespace SmarcGUI.MissionPlanning.Params
{
    public class AuvAltitudePointGUI : ParamGUI, IParamHasXZ, IParamHasY, IParamHasTolerance
    {
        [Header("AuvAltitudePointGUI")]
        public TMP_InputField LatField;
        public TMP_InputField LonField, TargetAltitudeField, MaxDepthField, RpmField, TimeoutField, ToleranceField;

        public double latitude
        {
            get { return ((AuvAltitudePoint)paramValue).latitude; }
            set
            {
                var gp = (AuvAltitudePoint)paramValue;
                gp.latitude = value;
                paramValue = gp;
                LatField.text = value.ToString();
                NotifyPathChange();
            }
        }
        public double longitude
        {
            get { return ((AuvAltitudePoint)paramValue).longitude; }
            set
            {
                var gp = (AuvAltitudePoint)paramValue;
                gp.longitude = value;
                paramValue = gp;
                LonField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float target_altitude
        {
            get { return ((AuvAltitudePoint)paramValue).target_altitude; }
            set
            {
                var d = (AuvAltitudePoint)paramValue;
                d.target_altitude = value;
                paramValue = d;
                TargetAltitudeField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float max_depth
        {
            get { return ((AuvAltitudePoint)paramValue).max_depth; }
            set
            {
                var d = (AuvAltitudePoint)paramValue;
                d.max_depth = value;
                paramValue = d;
                MaxDepthField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float rpm
        {
            get { return ((AuvAltitudePoint)paramValue).rpm; }
            set
            {
                var d = (AuvAltitudePoint)paramValue;
                d.rpm = value;
                paramValue = d;
                RpmField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float timeout
        {
            get { return ((AuvAltitudePoint)paramValue).timeout; }
            set
            {
                var d = (AuvAltitudePoint)paramValue;
                d.timeout = value;
                paramValue = d;
                TimeoutField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float tolerance
        {
            get { return ((AuvAltitudePoint)paramValue).tolerance; }
            set
            {
                var d = (AuvAltitudePoint)paramValue;
                d.tolerance = value;
                paramValue = d;
                ToleranceField.text = value.ToString();
                NotifyPathChange();
            }
        }


        protected override void SetupFields()
        {
            if (latitude == 0 && longitude == 0)
            {
                // set this to be the same as the previous geo point
                if (ParamIndex > 0)
                {
                    var previousGp = (AuvAltitudePoint)paramsList[ParamIndex - 1];
                    latitude = previousGp.latitude;
                    longitude = previousGp.longitude;
                    target_altitude = previousGp.target_altitude;
                    max_depth = previousGp.max_depth;
                    rpm = previousGp.rpm;
                    timeout = previousGp.timeout;
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

            if (target_altitude == 0) target_altitude = 1;
            if (tolerance == 0) tolerance = 1;

            LatField.text = latitude.ToString();
            LonField.text = longitude.ToString();
            TargetAltitudeField.text = target_altitude.ToString();
            MaxDepthField.text = max_depth.ToString();
            RpmField.text = rpm.ToString();
            TimeoutField.text = timeout.ToString();
            ToleranceField.text = tolerance.ToString();

            LatField.onEndEdit.AddListener(OnLatChanged);
            LonField.onEndEdit.AddListener(OnLonChanged);
            TargetAltitudeField.onEndEdit.AddListener(OnTargetAltitudeChanged);
            MaxDepthField.onEndEdit.AddListener(OnMaxDepthChanged);
            RpmField.onEndEdit.AddListener(OnRpmChanged);
            TimeoutField.onEndEdit.AddListener(OnTimeoutChanged);
            ToleranceField.onEndEdit.AddListener(OnToleranceChanged);

            fields.Add(LatField.GetComponent<RectTransform>());
            fields.Add(LonField.GetComponent<RectTransform>());
            fields.Add(TargetAltitudeField.GetComponent<RectTransform>());
            fields.Add(MaxDepthField.GetComponent<RectTransform>());
            fields.Add(RpmField.GetComponent<RectTransform>());
            fields.Add(TimeoutField.GetComponent<RectTransform>());
            fields.Add(ToleranceField.GetComponent<RectTransform>());

            OnSelectedChange();
        }

        public override List<string> GetFieldLabels()
        {
            return new List<string> { "Lat", "Lon", "T.Alt", "MaxDepth", "RPM", "T/O", "Tol" };
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

        void OnTargetAltitudeChanged(string s)
        {
            try { target_altitude = float.Parse(s); }
            catch
            {
                guiState.Log("Invalid target alt value");
                OnTargetAltitudeChanged(target_altitude.ToString());
                return;
            }
            NotifyPathChange();
        }

        void OnMaxDepthChanged(string s)
        {
            try { max_depth = float.Parse(s); }
            catch
            {
                guiState.Log("Invalid max depth value");
                OnMaxDepthChanged(max_depth.ToString());
                return;
            }
            NotifyPathChange();
        }

        void OnRpmChanged(string s)
        {
            try { rpm = float.Parse(s); }
            catch
            {
                guiState.Log("Invalid rpm value");
                OnRpmChanged(rpm.ToString());
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
            return -max_depth + target_altitude;
        }

        public float GetYReference()
        {
            return -max_depth;
        }

        public void SetY(float y)
        {
            target_altitude = y;
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
