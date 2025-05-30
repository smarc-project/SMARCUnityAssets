using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DefaultNamespace;
using SmarcGUI.WorldSpace;

namespace SmarcGUI
{
    public class RobotGUIOverlay : MonoBehaviour
    {
        [Header("Params")]
        public float FarAwayDistance = 50;
        float farDistSq;
        public float MinBoundingBoxSize = 30;

        [Header("Far-away Visualization")]
        public RectTransform FarawayVisualsRT;
        public RectTransform HeadingArrowRT;
        public RectTransform VelocityArrowRT;
        public Image PositionImg;

        [Header("Close-by Visualization")]
        public RectTransform BoundingBoxRT;
        public Image Top, Bottom, Left, Right;

        [Header("Colors")]
        public Color ColorMQTT = Color.red;
        public Color ColorROS = Color.green;
        public Color ColorSIM = Color.blue;

        [Header("Text")]
        public TMP_Text RobotNameText;

        [Header("Canvas")]
        public string UnderlayCanvasName = "Canvas-Under";
        Canvas underlayCanvas;

        Transform robotTF;
        Rigidbody robotRB;
        ArticulationBody robotAB;
        RobotGhost robotGhost;

        Renderer[] robotRenderers;
        GUIState guiState;

        void Awake()
        {
            underlayCanvas = GameObject.Find(UnderlayCanvasName).GetComponent<Canvas>();
            transform.SetParent(underlayCanvas.transform);
            guiState = FindFirstObjectByType<GUIState>();
            farDistSq = FarAwayDistance * FarAwayDistance;
        }

        void LateUpdate()
        {
            if(robotTF == null || guiState == null || guiState.CurrentCam == null)
            {
                BoundingBoxRT.gameObject.SetActive(false);
                RobotNameText.gameObject.SetActive(false);
                return;  
            }

            // check if the robot position is in front of the camera
            Vector3 toRobot = robotTF.position - guiState.CurrentCam.transform.position;
            var dot = Vector3.Dot(guiState.CurrentCam.transform.forward, toRobot);
            if (dot < 1)
            {
                BoundingBoxRT.gameObject.SetActive(false);
                RobotNameText.gameObject.SetActive(false);
                return;
            }
            else
            {
                RobotNameText.gameObject.SetActive(true);
            }

            Vector3 posDiff = robotTF.position - guiState.CurrentCam.transform.position;
            bool faraway = posDiff.sqrMagnitude > farDistSq;
            bool tooClose = posDiff.sqrMagnitude < 1;
            bool camTooLow = Mathf.Abs(posDiff.y) < 10;
            BoundingBoxRT.gameObject.SetActive(!faraway && !tooClose);
            FarawayVisualsRT.gameObject.SetActive(faraway && !camTooLow);
            if (faraway)
            {
                UpdateArrows();
                RobotNameText.rectTransform.anchoredPosition = PositionImg.rectTransform.anchoredPosition;
            }
            else
            {
                UpdateBBox();
                RobotNameText.rectTransform.anchoredPosition = BoundingBoxRT.anchoredPosition + new Vector2(0, BoundingBoxRT.sizeDelta.y/2 + 3);
            }
        }

        void UpdateArrows()
        {
            // we want to modify the position and rotation of the arrows
            // so that they point towards the robots velocity and heading
            // we need to do this in screen space
            var screenPos = Utils.WorldToCanvasPosition(underlayCanvas, guiState.CurrentCam, robotTF.position);
            PositionImg.rectTransform.anchoredPosition = screenPos;
            
            // first, find the heading of the robot
            Vector3 worldHeading;
            if(robotGhost != null)
            {
                worldHeading = robotGhost.ModelTF.forward;
            }
            else
            {
                worldHeading = robotTF.forward;
            }
            // then, project it onto y=0 plane
            worldHeading.y = 0;
            // then, project the tip of that vector to screen space
            var worldHeadingTip = robotTF.position + worldHeading;
            var screenHeadingTip = Utils.WorldToCanvasPosition(underlayCanvas, guiState.CurrentCam, worldHeadingTip);
            // then, find the angle of the vector in screen space
            var arrowAngle = Vector2.SignedAngle(Vector2.up, screenHeadingTip - screenPos);
            // then, rotate the HeadingarrowRT by that angle
            HeadingArrowRT.rotation = Quaternion.Euler(0, 0, arrowAngle);
            HeadingArrowRT.anchoredPosition = screenPos;


            Vector3 worldVel;
            if (robotAB != null)
            {
                worldVel = robotAB.linearVelocity;
            }
            else if (robotRB != null)
            {
                worldVel = robotRB.linearVelocity;
            }
            else if (robotGhost != null)
            {
                worldVel = robotGhost.velocity;
            }
            else
            {
                VelocityArrowRT.gameObject.SetActive(false);
                return;
            }

            if(worldVel.sqrMagnitude < 0.01*0.01)
            {
                VelocityArrowRT.gameObject.SetActive(false);
                return;
            }

            VelocityArrowRT.gameObject.SetActive(true);
            worldVel.y = 0;
            var worldVelTip = robotTF.position + worldVel;
            var screenVelTip = Utils.WorldToCanvasPosition(underlayCanvas, guiState.CurrentCam, worldVelTip);
            var velAngle = Vector2.SignedAngle(Vector2.up, screenVelTip - screenPos);
            VelocityArrowRT.rotation = Quaternion.Euler(0, 0, velAngle);
            VelocityArrowRT.anchoredPosition = screenPos;

            
        }

        void UpdateBBox()
        {
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
        }


        void SetColors(Color c)
        {
            Top.color = c;
            Bottom.color = c;
            Left.color = c;
            Right.color = c;
            PositionImg.color = c;
        }

        public void SetRobot(Transform robotTF, InfoSource infoSource)
        {
            this.robotTF = robotTF;
            if(robotTF.gameObject.TryGetComponent(out Rigidbody rb)) robotRB = rb;
            if(robotTF.gameObject.TryGetComponent(out ArticulationBody ab)) robotAB = ab;
            if(robotTF.gameObject.TryGetComponent(out RobotGhost rGhost)) robotGhost = rGhost;

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