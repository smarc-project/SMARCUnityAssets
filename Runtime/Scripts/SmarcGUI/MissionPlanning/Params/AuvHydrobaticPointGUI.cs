// using GeoRef;
// using SmarcGUI.WorldSpace;
// using TMPro;
// using UnityEngine;


// namespace SmarcGUI.MissionPlanning.Params
// {    public class AuvHydrobaticPointGUI : ParamGUI, IParamHasXZ, IParamHasY, IParamHasOrientation
//     {
//         [Header("AuvHydrobaticPointGUI")]
//         public TMP_InputField LatField;
//         public TMP_InputField LonField, TargetDepthField, TimeoutField;
        
//         [Header("Orientation")]
//         public TMP_InputField qwField, qxField, qyField, qzField;
//         public TMP_InputField exField, eyField, ezField;

//         GlobalReferencePoint globalReferencePoint;

//         public double latitude
//         {
//             get{return ((AuvHydrobaticPoint)paramValue).latitude; }
//             set{
//                 var gp = (AuvHydrobaticPoint)paramValue;
//                 gp.latitude = value;
//                 paramValue = gp;
//                 LatField.text = value.ToString();
//                 NotifyPathChange();
//             }
//         }
//         public double longitude
//         {
//             get{return ((AuvHydrobaticPoint)paramValue).longitude; }
//             set{
//                 var gp = (AuvHydrobaticPoint)paramValue;
//                 gp.longitude = value;
//                 paramValue = gp;
//                 LonField.text = value.ToString();
//                 NotifyPathChange();
//             }
//         }

//         public float target_depth
//         {
//             get { return ((AuvHydrobaticPoint)paramValue).target_depth; }
//             set
//             {
//                 var d = (AuvHydrobaticPoint)paramValue;
//                 d.target_depth = value;
//                 paramValue = d;
//                 TargetDepthField.text = value.ToString();
//                 NotifyPathChange();
//             }
//         }

//         public float timeout
//         {
//             get { return ((AuvHydrobaticPoint)paramValue).timeout; }
//             set
//             {
//                 var d = (AuvHydrobaticPoint)paramValue;
//                 d.timeout = value;
//                 paramValue = d;
//                 TimeoutField.text = value.ToString();
//                 NotifyPathChange();
//             }
//         }

//         public Orientation orientation
//         {
//             get { return ((AuvHydrobaticPoint)paramValue).orientation; }
//             set
//             {
//                 var oriParam = (AuvHydrobaticPoint)paramValue;
//                 oriParam.orientation = value;
//                 paramValue = oriParam;
//                 qwField.text = oriParam.w.ToString();
//                 qxField.text = oriParam.x.ToString();
//                 qyField.text = oriParam.y.ToString();
//                 qzField.text = oriParam.z.ToString();
//                 NotifyPathChange();
//             }
//         }

//         void Awake()
//         {
//             globalReferencePoint = FindFirstObjectByType<GlobalReferencePoint>();
//             guiState = FindFirstObjectByType<GUIState>();
//             qwField.interactable = false;
//             qxField.interactable = false;
//             qyField.interactable = false;
//             qzField.interactable = false;
//         }

//         protected override void SetupFields()
//         {
//             if(latitude == 0 && longitude == 0)
//             {
//                 // set this to be the same as the previous geo point
//                 if (ParamIndex > 0)
//                 {
//                     var previousGp = (AuvHydrobaticPoint)paramsList[ParamIndex - 1];
//                     latitude = previousGp.latitude;
//                     longitude = previousGp.longitude;
//                     target_depth = previousGp.target_depth;
//                     timeout = previousGp.timeout;
//                     guiState.Log("New LatLon set to previous.");
//                 }
//                 // if there is no previous geo point, set it to where the camera is looking at
//                 else
//                 {
//                     var point = guiState.GetLookAtPoint();
//                     var (lat, lon) = globalReferencePoint.GetLatLonFromUnityXZ(point.x, point.z);
//                     latitude = lat;
//                     longitude = lon;
//                     guiState.Log("New LatLon set to where the camera is looking at.");
//                 }
//             }

//             if(target_depth == 0) target_depth = -1;
//             if(min_altitude == 0) min_altitude = 1;

//             LatField.text = latitude.ToString();
//             LonField.text = longitude.ToString();
//             TargetDepthField.text = target_depth.ToString();
//             MinAltitudeField.text = min_altitude.ToString();
//             RpmField.text = rpm.ToString();
//             TimeoutField.text = timeout.ToString();

//             LatField.onEndEdit.AddListener(OnLatChanged);
//             LonField.onEndEdit.AddListener(OnLonChanged);
//             TargetDepthField.onEndEdit.AddListener(OnDepthChanged);
//             MinAltitudeField.onEndEdit.AddListener(OnMinAltitudeChanged);
//             RpmField.onEndEdit.AddListener(OnRpmChanged);
//             TimeoutField.onEndEdit.AddListener(OnTimeoutChanged);

//             fields.Add(LatField.GetComponent<RectTransform>());
//             fields.Add(LonField.GetComponent<RectTransform>());
//             fields.Add(TargetDepthField.GetComponent<RectTransform>());
//             fields.Add(MinAltitudeField.GetComponent<RectTransform>());
//             fields.Add(RpmField.GetComponent<RectTransform>());
//             fields.Add(TimeoutField.GetComponent<RectTransform>());

//             OnSelectedChange();
//         }


//         void OnLatChanged(string s)
//         {
//             try {latitude = double.Parse(s);}
//             catch 
//             {
//                 guiState.Log("Invalid latitude value");
//                 OnLatChanged(latitude.ToString());
//                 return;
//             }
//             NotifyPathChange();
//         }

//         void OnLonChanged(string s)
//         {
//             try{longitude = double.Parse(s);}
//             catch
//             {
//                 guiState.Log("Invalid longitude value");
//                 OnLonChanged(longitude.ToString());
//                 return;
//             }
//             NotifyPathChange();
//         }

//         void OnDepthChanged(string s)
//         {
//             try {target_depth = float.Parse(s);}
//             catch
//             {
//                 guiState.Log("Invalid depth value");
//                 OnDepthChanged(target_depth.ToString());
//                 return;
//             }
//             NotifyPathChange();
//         }

//         void OnMinAltitudeChanged(string s)
//         {
//             try {min_altitude = float.Parse(s);}
//             catch
//             {
//                 guiState.Log("Invalid min altitude value");
//                 OnMinAltitudeChanged(min_altitude.ToString());
//                 return;
//             }
//             NotifyPathChange();
//         }

//         void OnRpmChanged(string s)
//         {
//             try {rpm = float.Parse(s);}
//             catch
//             {
//                 guiState.Log("Invalid rpm value");
//                 OnRpmChanged(rpm.ToString());
//                 return;
//             }
//             NotifyPathChange();
//         }

//         void OnTimeoutChanged(string s)
//         {
//             try {timeout = float.Parse(s);}
//             catch
//             {
//                 guiState.Log("Invalid timeout value");
//                 OnTimeoutChanged(timeout.ToString());
//                 return;
//             }
//             NotifyPathChange();
//         }

        

//         public (float, float) GetXZ()
//         {
//             var (tx,tz) = globalReferencePoint.GetUnityXZFromLatLon(latitude, longitude);
//             return ((float)tx, (float)tz);
//         }

//         public void SetXZ(float x, float z)
//         {
//             var (lat, lon) = globalReferencePoint.GetLatLonFromUnityXZ(x, z);
//             latitude = lat;
//             longitude = lon;
//         }

//         public float GetY()
//         {
//             return -target_depth;
//         }

//         public float GetYReference()
//         {
//             return 0;
//         }

//         public void SetY(float y)
//         {
//             target_depth = -y;
//         }

//     }
// }
