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
            DescriptionField.onValueChanged.AddListener(desc => task.Description = desc);
            RunButton.onClick.AddListener(OnRunTask);
            RunButtonImage = RunButton.GetComponent<Image>();
            RunButtonText = RunButton.GetComponentInChildren<TMP_Text>();
            RunButtonOriginalColor = RunButtonImage.color;
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

            // instead of a foreach, we need to iterate over index because the param itself could modify the
            // individual parameter at this point
            for(int i=0; i<task.Params.Count; i++)
                InstantiateParam(Params.transform, task.Params, task.Params.Keys.ElementAt(i));

            UpdateHeight();
        }

        void InstantiateParam(Transform parent, Dictionary<string, object> taskParams, string paramKey)
        {
            if(missionPlanStore == null) missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            GameObject paramPrefab = missionPlanStore.GetParamPrefab(taskParams[paramKey]);
            GameObject paramGO = Instantiate(paramPrefab, parent);
            paramGO.GetComponent<ParamGUI>().SetParam(taskParams, paramKey, this);
        }


        // void ActuallyUpdateHeight()
        public void UpdateHeight()
        {
            float newHeight = baseHeight;
            if(Params.gameObject.activeSelf)
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
        }

        void OnDisable()
        {
            foreach (Transform child in Params.transform)
            {
                child.gameObject.SetActive(false);
            }
            guiState.UnregisterRobotSelectionChangedListener(this);
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
        }

        public List<Vector3> GetWorldPath()
        {
            var path = new List<Vector3>();
            foreach(Transform child in Params.transform)
            {
                if(child.TryGetComponent<IPathInWorld>(out var paramGUI)) path.AddRange(paramGUI.GetWorldPath());
            }
            return path;
        }

        public void OnParamChanged()
        {
            tstGUI.OnParamChanged();
            task.OnTaskModified();
        }
    }
}