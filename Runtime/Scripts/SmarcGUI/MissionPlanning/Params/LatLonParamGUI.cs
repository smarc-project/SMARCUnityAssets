using GeoRef;
using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;


namespace SmarcGUI.MissionPlanning.Params
{
    public class LatLonParamGUI : ParamGUI, IParamHasXZ
    {
        [Header("LatLonParamGUI")]
        public TMP_InputField LatField, LonField;

        GlobalReferencePoint globalReferencePoint;

        public double latitude
        {
            get{return ((LatLon)paramValue).latitude; }
            set{
                var gp = (LatLon)paramValue;
                gp.latitude = value;
                paramValue = gp;
                LatField.text = value.ToString();
                NotifyPathChange();
            }
        }
        public double longitude
        {
            get{return ((LatLon)paramValue).longitude; }
            set{
                var gp = (LatLon)paramValue;
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
            if(latitude == 0 && longitude == 0)
            {
                // set this to be the same as the previous geo point
                if (ParamIndex > 0)
                {
                    var previousGp = (LatLon)paramsList[ParamIndex - 1];
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

            UpdateTexts();

            LatField.onEndEdit.AddListener(OnLatChanged);
            LonField.onEndEdit.AddListener(OnLonChanged);
            fields.Add(LatField.GetComponent<RectTransform>());
            fields.Add(LonField.GetComponent<RectTransform>());
            

            OnSelectedChange();
        }

        void UpdateTexts()
        {
            LatField.text = latitude.ToString();
            LonField.text = longitude.ToString();
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
    }
}
