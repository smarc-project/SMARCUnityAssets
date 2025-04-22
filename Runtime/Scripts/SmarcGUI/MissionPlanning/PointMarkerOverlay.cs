using UnityEngine;
using UnityEngine.UI;
using DefaultNamespace;
using SmarcGUI.WorldSpace;

namespace SmarcGUI.MissionPlanning
{
    public class PointMarkerOverlay : MonoBehaviour
    {
        public RectTransform HeadingArrowRT;
        public Image PositionImg;

        public string UnderlayCanvasName = "Canvas-Under";
        Canvas underlayCanvas;

        PointMarker pointMarker;
        Transform pmTF;
        GUIState guiState;
        RectTransform rt;

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

        void LateUpdate()
        {   
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
            PositionImg.rectTransform.anchoredPosition = screenPos;

            var worldHeadingDeg = pointMarker.GetHeading();
            if(worldHeadingDeg == 400)
            {
                HeadingArrowRT.gameObject.SetActive(false);
                return;
            }
            var worldHeading = Quaternion.Euler(0, 0, worldHeadingDeg) * Vector3.up;
            var worldHeadingTip = pmTF.position + worldHeading;
            var screenHeadingTip = Utils.WorldToCanvasPosition(underlayCanvas, guiState.CurrentCam, worldHeadingTip);
            // then, find the angle of the vector in screen space
            var arrowAngle = Vector2.SignedAngle(Vector2.up, screenHeadingTip - screenPos);
            // then, rotate the HeadingarrowRT by that angle
            HeadingArrowRT.rotation = Quaternion.Euler(0, 0, arrowAngle);
            HeadingArrowRT.anchoredPosition = screenPos;
        }
    }

}