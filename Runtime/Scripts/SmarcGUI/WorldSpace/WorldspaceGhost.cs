using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public class WorldspaceGhost : MonoBehaviour, ICamChangeListener
    {

        public float IconHeight = 0.5f;
        public RectTransform PlanarIconCanvasRT;
        public RectTransform VetricalIconCanvasRT;
        public Transform ModelTF;
        public float FarAwayDistance = 50;
        public int ShowVerticalAngle = 20;

        GUIState guiState;
        Camera Cam;
        Vector3 originalCamDiff;
        float planarIconBaseScaleX, verticalIconBaseScaleX;
        float planarIconBaseScaleY, verticalIconBaseScaleY;


        void Awake()
        {
            guiState = FindFirstObjectByType<GUIState>();
            guiState.RegisterCamChangeListener(this);
            planarIconBaseScaleX = PlanarIconCanvasRT.localScale.x;
            planarIconBaseScaleY = PlanarIconCanvasRT.localScale.y;
            verticalIconBaseScaleX = VetricalIconCanvasRT.localScale.x;
            verticalIconBaseScaleY = VetricalIconCanvasRT.localScale.y;
            if(guiState.CurrentCam != null)
            {
                OnCamChange(guiState.CurrentCam);
            }
        }

        void OnDestroy()
        {
            guiState.UnregisterCamChangeListener(this);
        }


        void LateUpdate()
        {
            if(Cam == null) return;

            var camDiff = transform.position - Cam.transform.position;
            var camDist = camDiff.magnitude;
            // find the angle between the camera and the y=0 plane
            var camAngle = Vector3.Angle(camDiff, Vector3.up);
            
            ModelTF.gameObject.SetActive(camDist < FarAwayDistance);
            
            
            bool showPlanar = camAngle > 90+ShowVerticalAngle || camAngle < 90-ShowVerticalAngle;
            VetricalIconCanvasRT.gameObject.SetActive(camDist > FarAwayDistance && !showPlanar);
            PlanarIconCanvasRT.gameObject.SetActive(camDist > FarAwayDistance && showPlanar);
            
            if(PlanarIconCanvasRT.gameObject.activeSelf)
            {
                PlanarIconCanvasRT.position = new Vector3(transform.position.x, IconHeight, transform.position.z);
                var newPlanarScale = Vector3.one;
                newPlanarScale.x = Mathf.Max(planarIconBaseScaleX, planarIconBaseScaleX * camDist / originalCamDiff.magnitude);
                newPlanarScale.y = Mathf.Max(planarIconBaseScaleY, planarIconBaseScaleY * camDist / originalCamDiff.magnitude);
                PlanarIconCanvasRT.localScale = newPlanarScale;
            }
            if(VetricalIconCanvasRT.gameObject.activeSelf)
            {
                VetricalIconCanvasRT.position = new Vector3(transform.position.x, IconHeight, transform.position.z);
                VetricalIconCanvasRT.LookAt(new Vector3(Cam.transform.position.x, transform.position.y, Cam.transform.position.z));
                var newVerticalScale = Vector3.one;
                newVerticalScale.x = Mathf.Max(verticalIconBaseScaleX, verticalIconBaseScaleX * camDist / originalCamDiff.magnitude);
                newVerticalScale.y = Mathf.Max(verticalIconBaseScaleY, verticalIconBaseScaleY * camDist / originalCamDiff.magnitude);
                VetricalIconCanvasRT.localScale = newVerticalScale;
            }
            
        }

        public void OnCamChange(Camera newCam)
        {
            Cam = newCam;
            originalCamDiff = transform.position - Cam.transform.position;
        }
    }
}