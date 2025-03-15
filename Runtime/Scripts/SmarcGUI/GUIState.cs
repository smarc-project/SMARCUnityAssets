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

        [Header("Prefabs")]
        public GameObject RobotGuiPrefab;



        [Header("Defaults")]
        public Camera DefaultCamera;
        int defaultCameraIndex = 0;
        public float DefaultCameraLookAtMin = 1;
        public float DefaultCameraLookAtMax = 100;


        Dictionary<string, string> cameraTextToObjectPath;
        public Camera CurrentCam { get; private set; }
        List<RobotGUI> robotGUIs = new();
        public RobotGUI SelectedRobotGUI {get; private set;}
        public string SelectedRobotName => SelectedRobotGUI?.RobotName;
        
        WaterRenderToggle[] waterRenderToggles;
        bool renderWaters = true;



        List<ICamChangeListener> camChangeListeners = new();


        string CameraTextFromCamera(Camera c)
        {
            return $"{c.transform.root.name}/{c.name}";
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
            robotGUIs.Add(robotGui);
            return robotGui;
        }

        void InitRobotGuis()
        {
            GameObject[] robots = GameObject.FindGameObjectsWithTag("robot");
            foreach (var robot in robots) CreateNewRobotGUI(robot.transform.root.name, InfoSource.SIM, "-");
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

        public void OnRobotSelectionChanged(RobotGUI robotgui)
        {
            SelectedRobotGUI = robotgui.IsSelected? robotgui : null;
            foreach(var r in robotGUIs)
            {
                if(r != robotgui) r.Deselect();
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

    
        void OnGUI()
        {
            // UUID
            GUI.color = Color.white;
            GUI.Label(new Rect(0, Screen.height - 20, 400, 20), $"UUID: {UUID}");

            // Cursor position a small plus sign
            GUI.color = Color.red;
            float cursorSize = 30;
            float cursorWidth = 5;
            GUI.DrawTexture(new Rect(cursorX - cursorSize/2, cursorY - cursorWidth/2, cursorSize, cursorWidth), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cursorX - cursorWidth/2, cursorY - cursorSize/2, cursorWidth, cursorSize), Texture2D.whiteTexture);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MouseOnGUI = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MouseOnGUI = true;
        }



    }
}