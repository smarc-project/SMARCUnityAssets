using GeoRef;
using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;


namespace SmarcGUI.MissionPlanning.Params
{
    public class GeoPointParamGUI : ParamGUI, IParamHasXZ, IParamHasY
    {
        [Header("GeoPointParamGUI")]
        public TMP_InputField LatField, LonField, AltField;
        GlobalReferencePoint globalReferencePoint;

        public float altitude
        {
            get{return (float)((GeoPoint)paramValue).altitude; }
            set{
                var gp = (GeoPoint)paramValue;
                gp.altitude = value;
                paramValue = gp;
                AltField.text = value.ToString();
                NotifyPathChange();
            }
        }
        public double latitude
        {
            get{return ((GeoPoint)paramValue).latitude; }
            set{
                var gp = (GeoPoint)paramValue;
                gp.latitude = value;
                paramValue = gp;
                LatField.text = value.ToString();
                NotifyPathChange();
            }
        }
        public double longitude
        {
            get{return ((GeoPoint)paramValue).longitude; }
            set{
                var gp = (GeoPoint)paramValue;
                gp.longitude = value;
                paramValue = gp;
                LonField.text = value.ToString();
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
            if(altitude == 0 && latitude == 0 && longitude == 0)
            {
                // set this to be the same as the previous geo point
                if (ParamIndex > 0)
                {
                    var previousGp = (GeoPoint)paramsList[ParamIndex - 1];
                    latitude = previousGp.latitude;
                    longitude = previousGp.longitude;
                    altitude = previousGp.altitude;
                    guiState.Log("New GeoPoint set to previous.");
                }
                // if there is no previous geo point, set it to where the camera is looking at
                else
                {
                    var point = guiState.GetCameraLookAtPoint();
                    var (lat, lon) = globalReferencePoint.GetLatLonFromUnityXZ(point.x, point.z);
                    latitude = lat;
                    longitude = lon;
                    altitude = point.y;
                    guiState.Log("New GeoPoint set to where the camera is looking at.");
                }
            }

            UpdateTexts();

            LatField.onValueChanged.AddListener(OnLatChanged);
            LonField.onValueChanged.AddListener(OnLonChanged);
            AltField.onValueChanged.AddListener(OnAltChanged);

            OnSelectedChange();
        }

        void UpdateTexts()
        {
            LatField.text = latitude.ToString();
            LonField.text = longitude.ToString();
            AltField.text = altitude.ToString();
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

        void OnAltChanged(string s)
        {
            try{altitude = float.Parse(s);}
            catch
            {
                guiState.Log("Invalid altitude value");
                OnAltChanged(altitude.ToString());
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
            return altitude;
        }

        public void SetY(float y)
        {
            altitude = y;
        }
    }
}
