using UnityEngine;
using TMPro;
using System.Collections.Generic;

using UnityEngine.EventSystems;
using System.Linq;
using SmarcGUI.WorldSpace;
using SmarcGUI.MissionPlanning.Params;
using UnityEngine.UI;


namespace SmarcGUI.MissionPlanning.Tasks
{
    public class TaskGUI : MonoBehaviour, IHeightUpdatable, IRobotSelectionChangeListener, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler, IListItem, IPathInWorld, IParamChangeListener
    {
        public float BottomPadding = 5;
        public Task task;

        [Header("UI Elements")]
        public GameObject Params;
        public TMP_InputField DescriptionField;
        public TMP_Text TaskName;
        public RectTransform HighlightRT;
        public Button RunButton;
        public RectTransform WarningRT;

        [Header("Prefabs")]
        public GameObject ContextMenuPrefab;
        public GameObject PointMarkerPrefab;



        [Header("Worldspace")]
        public string WorldMarkersCollectionName = "WorldMarkers";
        Transform WorldMarkersCollection;
        PointMarker pointmarker = null;


        MissionPlanStore missionPlanStore;
        GUIState guiState;
        TSTGUI tstGUI;
        RectTransform rt;
        float baseHeight;
        Image RunButtonImage;
        Color RunButtonOriginalColor;
        TMP_Text RunButtonText;


        void Awake()
        {
            rt = GetComponent<RectTransform>();
            baseHeight = rt.sizeDelta.y;
            missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            guiState = FindFirstObjectByType<GUIState>();
            DescriptionField.onEndEdit.AddListener(desc => task.Description = desc);
            RunButton.onClick.AddListener(OnRunTask);
            RunButtonImage = RunButton.GetComponent<Image>();
            RunButtonText = RunButton.GetComponentInChildren<TMP_Text>();
            RunButtonOriginalColor = RunButtonImage.color;

            WorldMarkersCollection = GameObject.Find(WorldMarkersCollectionName).transform;
        }
        
        void OnRunTask()
        {
            var robotgui = guiState.SelectedRobotGUI;
            robotgui.SendStartTaskCommand(task);
        }

        public void SetTask(Task task, TSTGUI tstGUI)
        {
            this.task = task;
            this.tstGUI = tstGUI;
            TaskName.text = task.Name;
            DescriptionField.text = task.Description;
            
            guiState.RegisterRobotSelectionChangedListener(this);


            // we create world-markers for each parameter that actually has a position
            // if no parameter in the task has a position, it can not be visualized in the world
            // if a parameter has its horizontal position and vertical position defined by separate
            // parameters, then we first create the horizontal point, then _add_ the info from
            // following parameters onto the same point, until a new horizontal point containing parameter
            // is found. This allows tasks with split poses (like latlon + depth + oritentaiton as separate params)
            // Meaning, a task can split the 6DOF pose into 2+1+1+1+1 if it wants to...
            for(int i=0; i<task.Params.Count; i++)
            {
                var paramgui = InstantiateParamGui(Params.transform, task.Params, task.Params.Keys.ElementAt(i));
                if(paramgui is IParamHasXZ paramXZ)
                {
                    if(pointmarker != null)
                    {
                        // this task already has a marker?!
                        // so it defines _multiple_ horizontal points... individually.
                        // I am not going to handle this case! Make that a new interface!
                        Debug.LogError("Task has multiple parameters that implement IParamHasXZ, this is not supported!");
                        return;
                    }
                    var markerGO = Instantiate(PointMarkerPrefab, WorldMarkersCollection);
                    markerGO.name = $"{task.Name}_param_{i}";
                    pointmarker = markerGO.GetComponent<PointMarker>();
                    pointmarker.SetXZParam(paramXZ);
                }
            }

            UpdateHeight();

            // no marker, meaning we can not know where this task is in the world, so the rest of the params
            // wont be visualized in the world.
            if(pointmarker == null) return; 
            foreach(ParamGUI paramgui in Params.GetComponentsInChildren<ParamGUI>())
            {
                if(paramgui is IParamHasY paramY) pointmarker.SetYParam(paramY);
                if(paramgui is IParamHasHeading paramH) pointmarker.SetHeadingParam(paramH);
                if(paramgui is IParamHasOrientation paramO) pointmarker.SetOrientationParam(paramO);
            }
            
    
        }

        ParamGUI InstantiateParamGui(Transform parent, Dictionary<string, object> taskParams, string paramKey)
        {
            if(missionPlanStore == null) missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            GameObject paramPrefab = missionPlanStore.GetParamPrefab(taskParams[paramKey]);
            GameObject paramGO = Instantiate(paramPrefab, parent);
            var paramgui = paramGO.GetComponent<ParamGUI>();
            paramgui.SetParam(taskParams, paramKey, this);
            return paramgui;
        }


        public void UpdateHeight()
        {
            float newHeight = baseHeight;
            if(Params.activeSelf)
            {
                float paramsHeight = 0;
                foreach(Transform child in Params.transform)
                {
                    var paramRT = child.GetComponent<RectTransform>();
                    paramsHeight += paramRT.sizeDelta.y;
                }
                newHeight += paramsHeight + BottomPadding;
            }
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, newHeight);
            }


        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                var contextMenuGO = Instantiate(ContextMenuPrefab);
                var contextMenu = contextMenuGO.GetComponent<ListItemContextMenu>();
                contextMenu.SetItem(eventData.position, this);
            }

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HighlightRT.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HighlightRT.gameObject.SetActive(true);
        }

        public void OnRobotSelectionChange(RobotGUI SelectedRobotGUI)
        {
            RunButton.interactable = SelectedRobotGUI != null;
            if(SelectedRobotGUI == null)
            {
                WarningRT.gameObject.SetActive(false);
                RunButtonImage.color = RunButtonOriginalColor;
                RunButtonText.text = "Run";
            }
            else
            {
                // warning highlight if the selected robot does not have this task available
                if(SelectedRobotGUI.InfoSource == InfoSource.SIM) WarningRT.gameObject.SetActive(false);
                else WarningRT.gameObject.SetActive(!SelectedRobotGUI.TasksAvailableNames.Contains(task.Name));

                // make the RUN button green if it is already running this task
                // use the task uuid to check this, since many tasks of the same type can be running
                // 
                if(SelectedRobotGUI.TasksExecutingUuids.Contains(task.TaskUuid))
                {
                    RunButtonImage.color = Color.green;
                    RunButton.interactable = false;
                    RunButtonText.text = "Running";
                }
                else
                {
                    RunButtonImage.color = RunButtonOriginalColor;
                    RunButtonText.text = "Run";
                }
            } 
        }


        void OnEnable()
        {
            foreach (Transform child in Params.transform)
            {
                child.gameObject.SetActive(true);
            }
            UpdateHeight();
            guiState.RegisterRobotSelectionChangedListener(this);
            if(pointmarker != null) pointmarker.gameObject.SetActive(true);
        }

        void OnDisable()
        {
            foreach (Transform child in Params.transform)
            {
                child.gameObject.SetActive(false);
            }
            guiState.UnregisterRobotSelectionChangedListener(this);
            if(pointmarker != null) pointmarker.gameObject.SetActive(false);

        }

        public void OnListItemUp()
        {
            tstGUI.MoveTaskUp(this);
        }

        public void OnListItemDown()
        {
            tstGUI.MoveTaskDown(this);
        }

        public void OnListItemDelete()
        {
            guiState.UnregisterRobotSelectionChangedListener(this);
            tstGUI.DeleteTask(this);
            if(pointmarker != null) Destroy(pointmarker.gameObject);
        }

        public List<Vector3> GetWorldPath()
        {
            var path = new List<Vector3>();
            foreach(Transform child in Params.transform)
            {
                if(child.TryGetComponent<IPathInWorld>(out var paramGUI)) path.AddRange(paramGUI.GetWorldPath());
            }
            if(pointmarker != null) path.AddRange(pointmarker.GetWorldPath());
            return path;
        }

        public void OnParamChanged()
        {
            tstGUI.OnParamChanged();
            task.OnTaskModified();
            if(pointmarker != null) pointmarker.OnParamChanged();
        }
    }
}