using GeoRef;
using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;


namespace SmarcGUI.MissionPlanning.Params
{
    public class AuvDepthPointGUI : ParamGUI, IParamHasXZ, IParamHasY
    {
        [Header("AuvDepthPointGUI")]
        public TMP_InputField LatField;
        public TMP_InputField LonField, DepthField, MinAltitudeField, RpmField, TimeoutField;

        GlobalReferencePoint globalReferencePoint;

        public double latitude
        {
            get{return ((AuvDepthPoint)paramValue).latitude; }
            set{
                var gp = (AuvDepthPoint)paramValue;
                gp.latitude = value;
                paramValue = gp;
                LatField.text = value.ToString();
                NotifyPathChange();
            }
        }
        public double longitude
        {
            get{return ((AuvDepthPoint)paramValue).longitude; }
            set{
                var gp = (AuvDepthPoint)paramValue;
                gp.longitude = value;
                paramValue = gp;
                LonField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float depth
        {
            get { return ((AuvDepthPoint)paramValue).depth; }
            set
            {
                var d = (AuvDepthPoint)paramValue;
                d.depth = value;
                paramValue = d;
                DepthField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float min_altitude
        {
            get { return ((AuvDepthPoint)paramValue).min_altitude; }
            set
            {
                var d = (AuvDepthPoint)paramValue;
                d.min_altitude = value;
                paramValue = d;
                MinAltitudeField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float rpm
        {
            get { return ((AuvDepthPoint)paramValue).rpm; }
            set
            {
                var d = (AuvDepthPoint)paramValue;
                d.rpm = value;
                paramValue = d;
                RpmField.text = value.ToString();
                NotifyPathChange();
            }
        }

        public float timeout
        {
            get { return ((AuvDepthPoint)paramValue).timeout; }
            set
            {
                var d = (AuvDepthPoint)paramValue;
                d.timeout = value;
                paramValue = d;
                TimeoutField.text = value.ToString();
                NotifyPathChange();
            }
        }

        void Awake()
        {
            globalReferencePoint = FindFirstObjectByType<GlobalReferencePoint>();
            guiState = FindFirstObjectByType<GUIState>();
        }

        protected override void SetupFields()
        {
            if(latitude == 0 && longitude == 0)
            {
                // set this to be the same as the previous geo point
                if (ParamIndex > 0)
                {
                    var previousGp = (AuvDepthPoint)paramsList[ParamIndex - 1];
                    latitude = previousGp.latitude;
                    longitude = previousGp.longitude;
                    guiState.Log("New LatLon set to previous.");
                }
                // if there is no previous geo point, set it to where the camera is looking at
                else
                {
                    var point = guiState.GetLookAtPoint();
                    var (lat, lon) = globalReferencePoint.GetLatLonFromUnityXZ(point.x, point.z);
                    latitude = lat;
                    longitude = lon;
                    guiState.Log("New LatLon set to where the camera is looking at.");
                }
            }

            if(depth == 0) depth = -1;
            if(min_altitude == 0) min_altitude = 1;

            LatField.text = latitude.ToString();
            LonField.text = longitude.ToString();
            DepthField.text = depth.ToString();
            MinAltitudeField.text = min_altitude.ToString();
            RpmField.text = rpm.ToString();
            TimeoutField.text = timeout.ToString();

            LatField.onEndEdit.AddListener(OnLatChanged);
            LonField.onEndEdit.AddListener(OnLonChanged);
            DepthField.onEndEdit.AddListener(OnDepthChanged);
            MinAltitudeField.onEndEdit.AddListener(OnMinAltitudeChanged);
            RpmField.onEndEdit.AddListener(OnRpmChanged);
            TimeoutField.onEndEdit.AddListener(OnTimeoutChanged);

            fields.Add(LatField);
            fields.Add(LonField);
            fields.Add(DepthField);
            fields.Add(MinAltitudeField);
            fields.Add(RpmField);
            fields.Add(TimeoutField);

            OnSelectedChange();
        }


        void OnLatChanged(string s)
        {
            try {latitude = double.Parse(s);}
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
            try{longitude = double.Parse(s);}
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
            try {depth = float.Parse(s);}
            catch
            {
                guiState.Log("Invalid depth value");
                OnDepthChanged(depth.ToString());
                return;
            }
            NotifyPathChange();
        }

        void OnMinAltitudeChanged(string s)
        {
            try {min_altitude = float.Parse(s);}
            catch
            {
                guiState.Log("Invalid min altitude value");
                OnMinAltitudeChanged(min_altitude.ToString());
                return;
            }
            NotifyPathChange();
        }

        void OnRpmChanged(string s)
        {
            try {rpm = float.Parse(s);}
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
            try {timeout = float.Parse(s);}
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
            var (tx,tz) = globalReferencePoint.GetUnityXZFromLatLon(latitude, longitude);
            return ((float)tx, (float)tz);
        }

        public void SetXZ(float x, float z)
        {
            var (lat, lon) = globalReferencePoint.GetLatLonFromUnityXZ(x, z);
            latitude = lat;
            longitude = lon;
        }

        public float GetY()
        {
            return -depth;
        }

        public void SetY(float y)
        {
            depth = -y;
        }

    }
}
