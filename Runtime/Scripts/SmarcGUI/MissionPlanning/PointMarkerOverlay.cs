using UnityEngine;
using UnityEngine.UI;
using DefaultNamespace;
using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine.EventSystems;

namespace SmarcGUI.MissionPlanning
{

    public class PointMarkerOverlay : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [Header("Components")]
        public RectTransform HeadingArrowRT;
        public Image PositionImg;
        public Image HighlightImg;
        public RectTransform FloatingNameCanvas;
        public TMP_Text FloatingNameText;
        public TMP_Text FloatingDescriptionText;
        public RectTransform FloatingNameBackgroundRT;

        [Tooltip("Where this marker will be drawn on")]
        public string UnderlayCanvasName = "Canvas-Under";
        Canvas underlayCanvas;

        [Header("Drag Settings")]
        [Tooltip("The button to use for dragging")]
        public PointerEventData.InputButton Button = PointerEventData.InputButton.Left;


        PointMarker pointMarker;
        Transform pmTF;
        GUIState guiState;
        RectTransform rt;
        bool dragging = false;

        void Awake()
        {
            guiState = FindFirstObjectByType<GUIState>();
            underlayCanvas = GameObject.Find(UnderlayCanvasName).GetComponent<Canvas>();
            transform.SetParent(underlayCanvas.transform);
            rt = GetComponent<RectTransform>();
            rt.position = underlayCanvas.transform.position;
        }

        public void SetPointMarker(PointMarker pointMarker)
        {
            this.pointMarker = pointMarker;
            pmTF = pointMarker.transform;
        }

        public void Highlight(bool highlight)
        {
            HighlightImg.gameObject.SetActive(highlight && gameObject.activeSelf);
        }

        void LateUpdate()
        {   
            if(dragging) return;

            // check if the marker position is in front of the camera
            Vector3 toMarker = pmTF.position - guiState.CurrentCam.transform.position;
            var dot = Vector3.Dot(guiState.CurrentCam.transform.forward, toMarker);
            if (dot < 1)
            {
                PositionImg.gameObject.SetActive(false);
                HeadingArrowRT.gameObject.SetActive(false);
                return;
            }
            PositionImg.gameObject.SetActive(true);

            bool camTooLow = Mathf.Abs(toMarker.y) < 10;
            HeadingArrowRT.gameObject.SetActive(!camTooLow);
            
            var screenPos = Utils.WorldToCanvasPosition(underlayCanvas, guiState.CurrentCam, pmTF.position);
            rt.anchoredPosition = screenPos;

            var worldHeadingVec = pointMarker.GetHeadingVec();
            if(worldHeadingVec == Vector3.zero)
            {
                HeadingArrowRT.gameObject.SetActive(false);
                return;
            }
            var worldHeadingTip = pmTF.position + worldHeadingVec;
            var screenHeadingTip = Utils.WorldToCanvasPosition(underlayCanvas, guiState.CurrentCam, worldHeadingTip);
            // then, find the angle of the vector in screen space
            var arrowAngle = Vector2.SignedAngle(Vector2.up, screenHeadingTip - screenPos);
            // then, rotate the HeadingarrowRT by that angle
            HeadingArrowRT.rotation = Quaternion.Euler(0, 0, arrowAngle);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != Button) return;
            if (guiState.CurrentCam == null) return;

            dragging = true;
            guiState.MouseDragging = true;
            rt.anchoredPosition += eventData.delta;
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            // project the screen position of the overlay object back to world space
            // so we can set the point marker position to that world position
            Ray camRay = guiState.CurrentCam.ScreenPointToRay(rt.position);
            Plane plane = new(pmTF.up, pmTF.position);
            plane.Raycast(camRay, out float camPlaneDist);
            Vector3 newPos = camRay.origin + camRay.direction * camPlaneDist;
            pointMarker.transform.position = newPos;
            pointMarker.OnWorldDragEnd(DragConstraint.XZ); // only move along XZ!

            dragging = false;
            guiState.MouseDragging = false;
        }

    }

}