using System.Linq;
using TMPro;
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

        PointMarker pointMarker;
        Transform pmTF;

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
                gameObject.SetActive(false);
                return;
            }

            // basically copy RobotGUIOverlay UpdateArrows() here.
            
        }
    }

}