using UnityEngine;
using TMPro;
using System.Collections.Generic;

using Utils = DefaultNamespace.Utils;
using VehicleComponents.Sensors;
using UnityEngine.EventSystems;
using SmarcGUI.Water;
using UnityEngine.UI;



namespace SmarcGUI
{

    public class GUIState : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
    {
        public string UUID{get; private set;}
        public bool MouseOnGUI{get; private set;}

        [Tooltip("Cursor position in normalized coordinates on the screen (0-1)")]
        public Vector2 CursorInView => new(0.5f, 0.5f);
        float cursorX => Screen.width*CursorInView.x;
        float cursorY => Screen.height*CursorInView.y;


        [Header("GUI Elements")]
        public TMP_Dropdown cameraDropdown;
        public TMP_Text LogText;
        public RectTransform RobotsScrollContent;
        public Button ToggleWaterRenderButton;
        public TMP_Text ComapssText;

        [Header("Prefabs")]
        public GameObject RobotGuiPrefab;



        [Header("Defaults")]
        public Camera DefaultCamera;
        int defaultCameraIndex = 0;
        public float DefaultCameraLookAtMin = 1;
        public float DefaultCameraLookAtMax = 100;


        Dictionary<string, string> cameraTextToObjectPath;
        public Camera CurrentCam { get; private set; }
        public Dictionary<string, RobotGUI> RobotGuis = new();
        public RobotGUI SelectedRobotGUI {get; private set;}
        public string SelectedRobotName => SelectedRobotGUI?.RobotName;
        
        WaterRenderToggle[] waterRenderToggles;
        bool renderWaters = true;



        List<ICamChangeListener> camChangeListeners = new();
        List<IRobotSelectionChangeListener> robotSelectionChangeListeners = new();


        string CameraTextFromCamera(Camera c)
        {
            var robot = Utils.FindParentWithTag(c.gameObject, "robot", false);
            if(robot == null)
            {
                return $"{c.transform.parent.name}/{c.name}";
            }
            else
            {
                return $"{robot.name}/{c.name}";
            }
        }

        void InitCameraDropdown()
        {
            cameraDropdown.onValueChanged.AddListener(OnCameraChanged);

            cameraTextToObjectPath = new Dictionary<string, string>();
            // disable all cams except the "main cam" at the start
            Camera[] cams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach(Camera c in cams)
            {
                // dont mess with sensor cameras
                if(c.gameObject.TryGetComponent<Sensor>(out Sensor s)) continue;
                // disable all cams by default. we will enable one later.
                c.enabled = false;
                // disable all audiolisteners. we got no audio. we wont enable these.
                if(c.gameObject.TryGetComponent<AudioListener>(out AudioListener al)) al.enabled=false;
                
                string objectPath = Utils.GetGameObjectPath(c.gameObject);
                string ddText = CameraTextFromCamera(c);
                cameraTextToObjectPath.Add(ddText, objectPath);
                cameraDropdown.options.Add(new TMP_Dropdown.OptionData(){text=ddText});
            }

            for (int i = 0; i < cameraDropdown.options.Count; i++)
            {
                if (cameraDropdown.options[i].text == CameraTextFromCamera(DefaultCamera))
                {
                    defaultCameraIndex = i;
                    break;
                }
            }
            SelectDefaultCamera();
        }

        public void SelectDefaultCamera()
        {
            cameraDropdown.value = defaultCameraIndex;
            cameraDropdown.RefreshShownValue();
            OnCameraChanged(cameraDropdown.value);
        }


        public RobotGUI CreateNewRobotGUI(string robotName, InfoSource infoSource, string robotNamespace)
        {
            var robotGui = Instantiate(RobotGuiPrefab, RobotsScrollContent).GetComponent<RobotGUI>();
            robotGui.SetRobot(robotName, infoSource, robotNamespace);
            RobotGuis[robotName] = robotGui;
            Log($"Created new RobotGUI for {robotName}");
            return robotGui;
        }

        public void RemoveRobotGUI(string robotName)
        {
            RobotGuis.Remove(robotName);
        }

        void InitRobotGuis()
        {
            GameObject[] robots = GameObject.FindGameObjectsWithTag("robot");
            foreach (var robot in robots) 
            {
                CreateNewRobotGUI(robot.name, InfoSource.SIM, "-");
            }   
        }


        public void RegisterCamChangeListener(ICamChangeListener listener)
        {
            camChangeListeners.Add(listener);
        }

        public void UnregisterCamChangeListener(ICamChangeListener listener)
        {
            camChangeListeners.Remove(listener);
        }


        public void OnCameraChanged(int camIndex)
        {
            var selection = cameraDropdown.options[camIndex];
            string objectPath = cameraTextToObjectPath[selection.text];
            GameObject selectedGO = GameObject.Find(objectPath);
            if(selectedGO == null) return;

            if(CurrentCam != null) CurrentCam.enabled = false;
            CurrentCam = selectedGO.GetComponent<Camera>();
            CurrentCam.enabled = true;

            foreach(var listener in camChangeListeners)
            {
                listener.OnCamChange(CurrentCam);
            }
        }


        public void RegisterRobotSelectionChangedListener(IRobotSelectionChangeListener listener)
        {
            if(listener == null) return;
            if(robotSelectionChangeListeners.Contains(listener)) return;
            robotSelectionChangeListeners.Add(listener);
        }

        public void UnregisterRobotSelectionChangedListener(IRobotSelectionChangeListener listener)
        {
            if(listener == null) return;
            if(!robotSelectionChangeListeners.Contains(listener)) return;
            robotSelectionChangeListeners.Remove(listener);
        }


        public void OnRobotSelectionChanged(RobotGUI robotgui)
        {
            SelectedRobotGUI = robotgui.IsSelected? robotgui : null;
            foreach(var r in RobotGuis)
            {
                if(r.Value.RobotName != robotgui.RobotName) r.Value.Deselect();
            }

            foreach(var listener in robotSelectionChangeListeners)
            {
                listener.OnRobotSelectionChange(SelectedRobotGUI);
            }
        }
        

        public void Log(string text)
        {
            string currentTime = System.DateTime.Now.ToString("HH:mm:ss");
            LogText.text = $"[{currentTime}] {text}\n{LogText.text}";
            if(LogText.text.Length > 5000)
            {
                LogText.text = LogText.text[..1000];
            }
            Debug.Log(text);
        }


        public Vector3 GetCameraLookAtPoint()
        {
            Ray ray = CurrentCam.ScreenPointToRay(new Vector3(cursorX, cursorY, 0));
            Plane zeroPlane = new(Vector3.up, Vector3.zero);
            var dist = 10f;
            bool hitWater = false;
            if (zeroPlane.Raycast(ray, out float camToPlaneDist))
            {
                // dont want it too far...
                dist = camToPlaneDist;
                hitWater = true;
            }
            if(!hitWater)
            {
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    dist = hit.distance;
                }
            }
            dist = Mathf.Clamp(dist, DefaultCameraLookAtMin, DefaultCameraLookAtMax);
            return ray.GetPoint(dist);
        }


        void Start()
        {
            if(DefaultCamera == null) DefaultCamera = Camera.main;
            UUID = System.Guid.NewGuid().ToString();
            InitCameraDropdown();
            InitRobotGuis();
            waterRenderToggles = FindObjectsByType<WaterRenderToggle>(FindObjectsSortMode.None);
            ToggleWaterRenderButton.onClick.AddListener(() => {
                foreach(var toggle in waterRenderToggles)
                {
                    renderWaters = !renderWaters;
                    toggle.ToggleWaterRender(renderWaters);
                }
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MouseOnGUI = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MouseOnGUI = true;
        }

        void LateUpdate()
        {
            // Update the compass text WRT the current camera
            if(CurrentCam == null) ComapssText.text = "NO CAM";
            else
            {
                Vector3 camForward = CurrentCam.transform.forward;
                // check if camera is looking straight up or down
                // if camForward.y is 0, then the camera is looking straight up or down
                if(Mathf.Abs(camForward.x) < 0.1f && Mathf.Abs(camForward.z) < 0.1f)
                {
                    // camera looking straigh up or down...
                    ComapssText.text = "UP/DOWN";
                    return;
                }
                // calculate the angle the camera is looking towards
                // and turn it into a compass direction
                // project forward onto the xz plane
                camForward.y = 0;
                float angle = Vector3.SignedAngle(Vector3.forward, camForward, Vector3.up);
                // convert to compass direction
                string[] compassDirections = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
                int index = Mathf.RoundToInt((angle + 360) / 45) % 8;
                ComapssText.text = $"{angle:F1}° ({compassDirections[index]})";
            }
        }



    }
}