using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DefaultNamespace;

namespace SmarcGUI
{
    public class RobotGUIOverlay : MonoBehaviour
    {
        [Header("Params")]
        public float FarAwayDistance = 50;
        public float MinBoundingBoxSize = 30;

        [Header("Borders")]
        public RectTransform BoundingBoxRT;
        public Image Top, Bottom, Left, Right;
        public Color ColorMQTT = Color.red;
        public Color ColorROS = Color.green;
        public Color ColorSIM = Color.blue;

        [Header("Text")]
        public TMP_Text RobotNameText;

        [Header("Canvas")]
        public string UnderlayCanvasName = "Canvas-Under";
        Canvas underlayCanvas;

        Transform robotTF;
        Renderer[] robotRenderers;
        GUIState guiState;

        void Awake()
        {
            underlayCanvas = GameObject.Find(UnderlayCanvasName).GetComponent<Canvas>();
            transform.SetParent(underlayCanvas.transform);
            guiState = FindFirstObjectByType<GUIState>();
        }

        void OnGUI()
        {
            if(robotTF == null)
            {
                BoundingBoxRT.gameObject.SetActive(false);
                RobotNameText.gameObject.SetActive(false);
                return;  
            }

            var camDist = Vector3.Distance(robotTF.position, guiState.CurrentCam.transform.position);
            if (camDist > FarAwayDistance)
            {
                BoundingBoxRT.gameObject.SetActive(false);
                RobotNameText.gameObject.SetActive(false);
                return;
            }
            
            BoundingBoxRT.gameObject.SetActive(true);
            RobotNameText.gameObject.SetActive(true);

            Bounds maxBounds = new(robotTF.position, new(1,1,1));
            // find the max bounds from all the renderers so that we can put the overlay
            // in a way that it doesnt obstruct the robot visuals
            // hopefully people dont have models with 10k renderers... i'm looking at you cad-people...
            if (robotRenderers.Length > 0)
            {
                maxBounds = robotRenderers[0].bounds;
                foreach (var renderer in robotRenderers)
                {
                    maxBounds.Encapsulate(renderer.bounds);
                }
            }

            // and we set the size to be the same as the max bounds
            // but we need this in cam space too
            // so we need to transform extents of the bounding box to cam space
            // then find the min/max in cam space
            Vector3[] pts = new Vector3[8];
            pts[0] = maxBounds.min;
            pts[1] = maxBounds.max;
            pts[2] = new Vector3(pts[0].x, pts[0].y, pts[1].z);
            pts[3] = new Vector3(pts[0].x, pts[1].y, pts[0].z);
            pts[4] = new Vector3(pts[1].x, pts[0].y, pts[0].z);
            pts[5] = new Vector3(pts[0].x, pts[1].y, pts[1].z);
            pts[6] = new Vector3(pts[1].x, pts[0].y, pts[1].z);
            pts[7] = new Vector3(pts[1].x, pts[1].y, pts[0].z);
            // move into cam space
            Vector2[] camPts = new Vector2[8];
            for(int i=0; i<8; i++)
            {
                camPts[i] = Utils.WorldToCanvasPosition(underlayCanvas, guiState.CurrentCam, pts[i]);
                // draw the pts on screen for debugging
                // GUI.Label(new Rect(Screen.width/2 + camPts[i].x, Screen.height/2 - camPts[i].y, 50, 50), i.ToString());
            }
            // find min/max
            var min = camPts.Aggregate((min, next) => new Vector2(Mathf.Min(min.x, next.x), Mathf.Min(min.y, next.y)));
            var max = camPts.Aggregate((max, next) => new Vector2(Mathf.Max(max.x, next.x), Mathf.Max(max.y, next.y)));
            // GUI.Label(new Rect(Screen.width/2 + min.x, Screen.height/2 - min.y, 50, 50), "min");
            // GUI.Label(new Rect(Screen.width/2 + max.x, Screen.height/2 - max.y, 50, 50), "max");
            var size = max - min;
            BoundingBoxRT.sizeDelta = new Vector2(Mathf.Max(size.x, MinBoundingBoxSize), Mathf.Max(size.y, MinBoundingBoxSize));
            // then we put the overlay where the center is, in camera space
            BoundingBoxRT.anchoredPosition = (min + max) / 2;
            RobotNameText.rectTransform.anchoredPosition = BoundingBoxRT.anchoredPosition + new Vector2(0, BoundingBoxRT.sizeDelta.y/2 + 3);
        }


        void SetColors(Color c)
        {
            Top.color = c;
            Bottom.color = c;
            Left.color = c;
            Right.color = c;
        }

        public void SetRobot(Transform robotTF, InfoSource infoSource)
        {
            this.robotTF = robotTF;
            robotRenderers = robotTF.GetComponentsInChildren<Renderer>();
            switch (infoSource)
            {
                case InfoSource.MQTT:
                    SetColors(ColorMQTT);
                    RobotNameText.text = $"{robotTF.name} (MQTT)";
                    break;
                case InfoSource.ROS:
                    SetColors(ColorROS);
                    RobotNameText.text = $"{robotTF.name} (ROS)";
                    break;
                case InfoSource.SIM:
                    SetColors(ColorSIM);
                    RobotNameText.text = $"{robotTF.name} (SIM)";
                    break;
            }
        }

    }
}