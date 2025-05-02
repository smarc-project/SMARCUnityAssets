using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SmarcGUI.MissionPlanning;
using TMPro;

namespace SmarcGUI.WorldSpace
{
    public class PointMarker : MonoBehaviour, IWorldDraggable, IParamChangeListener, IPathInWorld
    {
        IParamHasXZ paramXZ;
        IParamHasY paramY;
        IParamHasHeading paramHeading;
        IParamHasOrientation paramOrientation;

        [Header("3D visuals")]
        public GameObject dragArrows;
        GameObject arrowYup, arrowYdown;
        public Transform headingCone;
        public Transform orientationModel;
        public GameObject HighlightObject;
        public Canvas FloatingNameCanvas;
        public TMP_Text FloatingNameText;
        public TMP_Text FloatingDescriptionText;


        LineRenderer lineToShadow;
        public Transform shadowMarker;

        GUIState guiState;
        bool isSelected = false;    
        bool isFar = false;

        [Header("2D visuals")]
        public float FarAwayDistance = 50;
        float farAwayDistSq;
        public GameObject PointMarkerOverlayPrefab;
        PointMarkerOverlay overlay;



        void Awake()
        {
            guiState = FindFirstObjectByType<GUIState>();
            farAwayDistSq = FarAwayDistance * FarAwayDistance;

            var das = dragArrows.GetComponent<DragArrows>();
            arrowYup = das.PY.gameObject;
            arrowYdown = das.NY.gameObject;

            lineToShadow = shadowMarker.GetComponent<LineRenderer>();
            lineToShadow.startWidth = 0.1f;
            lineToShadow.endWidth = 0.1f;
            lineToShadow.startColor = Color.yellow;
            lineToShadow.endColor = Color.yellow;
            lineToShadow.positionCount = 2;

            var overlayGO = Instantiate(PointMarkerOverlayPrefab);
            overlay = overlayGO.GetComponent<PointMarkerOverlay>();
            overlay.SetPointMarker(this);

            UpdateWidgets();
        }


        public void OnWorldDrag(Vector3 deltaPos)
        {
            transform.position += deltaPos;
        }

        public void OnWorldDragEnd(DragConstraint dragConstraint)
        {
            if(dragConstraint == DragConstraint.XZ ||
               dragConstraint == DragConstraint.X  || 
               dragConstraint == DragConstraint.Z)
            {
                paramXZ?.SetXZ(transform.position.x, transform.position.z);
            }
            else if(dragConstraint == DragConstraint.Y)
            {
                paramY?.SetY(transform.position.y);
            }
            
        }

        public void SetNameDesc(string name, string desc)
        {
            FloatingNameCanvas.gameObject.SetActive(!string.IsNullOrEmpty(name));
            FloatingNameText.text = name;
            FloatingDescriptionText.text = desc;
            overlay.FloatingNameText.text = name;
            overlay.FloatingDescriptionText.text = desc;
            overlay.FloatingNameCanvas.gameObject.SetActive(!string.IsNullOrEmpty(name));
        }

        public void SetXZParam(IParamHasXZ param)
        {
            if(param == null) return;
            paramXZ = param;
            var (x, z) = param.GetXZ();
            transform.position = new Vector3(x, transform.position.y, z);
            if(shadowMarker != null)
            {
                shadowMarker.position = new Vector3(transform.position.x, 0, transform.position.z);
                lineToShadow.SetPosition(0, transform.position);
                lineToShadow.SetPosition(1, shadowMarker.position);
            }
        }

        public void SetYParam(IParamHasY param)
        {
            if(param == null) return;
            paramY = param;
            var y = param.GetY();
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        public void SetHeadingParam(IParamHasHeading param)
        {
            if(param == null) return;
            paramHeading = param;
            headingCone.localEulerAngles = new Vector3(0, param.GetHeading(), 0);
        }

        public void SetOrientationParam(IParamHasOrientation param)
        {
            if(param == null) return;
            paramOrientation = param;
            var q = param.GetUnityQuaternion();
            orientationModel.localRotation = q;
            orientationModel.gameObject.SetActive(paramXZ != null);
        }

        public void OnParamChanged()
        {
            SetYParam(paramY);
            SetXZParam(paramXZ);
            SetHeadingParam(paramHeading);
            SetOrientationParam(paramOrientation);
        }

        public List<Vector3> GetWorldPath()
        {
            var l = new List<Vector3>();
            if(paramXZ != null) l.Add(transform.position);
            return l;
        }

        public Vector3 GetHeadingVec()
        {
            if(paramHeading != null) return headingCone.forward;
            if(paramOrientation != null) return orientationModel.forward;
            return Vector3.zero;
        }

        void UpdateWidgets()
        {
            var draw3Dwidgets = !isFar;

            if(paramXZ == null) draw3Dwidgets = false;
            overlay.gameObject.SetActive(!draw3Dwidgets);

            dragArrows.SetActive(draw3Dwidgets && isSelected);
            headingCone.gameObject.SetActive(paramHeading != null && draw3Dwidgets);
            orientationModel.gameObject.SetActive(paramOrientation != null && draw3Dwidgets);
            
            arrowYup.SetActive(paramY != null && draw3Dwidgets);
            arrowYdown.SetActive(paramY != null && draw3Dwidgets);
            var y = paramY != null ? paramY.GetY() : 0;
            shadowMarker.gameObject.SetActive(Mathf.Abs(y) > 1 && draw3Dwidgets);
            lineToShadow.enabled = Mathf.Abs(y) > 1  && draw3Dwidgets;

            FloatingNameCanvas.gameObject.SetActive(draw3Dwidgets);
            FloatingNameCanvas.transform.rotation = guiState.CurrentCam.transform.rotation;
        }

        public void Highlight(bool on)
        {
            HighlightObject.SetActive(on && !isFar);
            overlay.Highlight(on && isFar);
        }

        public void Selected(bool on)
        {   
            dragArrows.SetActive(on && !isFar);
            isSelected = on;
        }

        void LateUpdate()
        {
            // disable all 3D stuff if the camera is far away and replace them with screen-space 2D markers
            // similar to how we display robot ghosts.
            if(guiState.CurrentCam == null) return;
            if(guiState.MouseDragging) return; //also dont do anything if we are dragging something...
            var camDiff = transform.position - guiState.CurrentCam.transform.position;
            isFar = camDiff.sqrMagnitude > farAwayDistSq;
            UpdateWidgets();
        }
    }
}