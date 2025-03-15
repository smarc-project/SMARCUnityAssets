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
    public class TaskGUI : MonoBehaviour, IHeightUpdatable, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler, IListItem, IPathInWorld, IParamChangeListener
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
        Image RunButtonImage;
        Color RunButtonOriginalColor;
        TMP_Text RunButtonText;

        bool needsHeightUpdate = false;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
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

            // instead of a foreach, we need to iterate over index because the param itself could modify the
            // individual parameter at this point
            for(int i=0; i<task.Params.Count; i++)
                InstantiateParam(Params.transform, task.Params, task.Params.Keys.ElementAt(i));

            UpdateHeight();
        }

        void InstantiateParam(Transform parent, Dictionary<string, object> taskParams, string paramKey)
        {
            GameObject paramGO;
            GameObject paramPrefab = missionPlanStore.GetParamPrefab(taskParams[paramKey]);
            paramGO = Instantiate(paramPrefab, parent);
            paramGO.GetComponent<ParamGUI>().SetParam(taskParams, paramKey, this);
        }


        public void UpdateHeight()
        {
            // Why? because this is under a scroll view and we cant have size-fitter component without problems
            // this seems to let the scroll view do its thing, and then update the size after.
            // Basically delaying the update by one frame.
            needsHeightUpdate = true;
        }

        void ActuallyUpdateHeight()
        {
            float totalHeight = 0;
            var paramsRT = Params.GetComponent<RectTransform>();
            totalHeight += paramsRT.sizeDelta.y;
            var nameRT = TaskName.GetComponent<RectTransform>();
            totalHeight += nameRT.sizeDelta.y;
            var descRT = DescriptionField.GetComponent<RectTransform>();
            totalHeight += descRT.sizeDelta.y;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, totalHeight + BottomPadding);
            needsHeightUpdate = false;
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

        void OnGUI()
        {
            if(needsHeightUpdate) ActuallyUpdateHeight();
            RunButton.interactable = guiState.SelectedRobotGUI != null;
            if(guiState.SelectedRobotGUI == null)
            {
                WarningRT.gameObject.SetActive(false);
                RunButtonImage.color = RunButtonOriginalColor;
                RunButtonText.text = "Run";
            }
            else
            {
                // warning highlight if the selected robot does not have this task available
                if(guiState.SelectedRobotGUI.InfoSource == InfoSource.SIM) WarningRT.gameObject.SetActive(false);
                else WarningRT.gameObject.SetActive(!guiState.SelectedRobotGUI.TasksAvailableNames.Contains(task.Name));

                // make the RUN button green if it is already running this task
                // use the task uuid to check this, since many tasks of the same type can be running
                // 
                if(guiState.SelectedRobotGUI.TasksExecutingUuids.Contains(task.TaskUuid))
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
        }

        void OnDisable()
        {
            foreach (Transform child in Params.transform)
            {
                child.gameObject.SetActive(false);
            }
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
            tstGUI.DeleteTask(this);
        }

        public List<Vector3> GetWorldPath()
        {
            var path = new List<Vector3>();
            foreach(Transform child in Params.transform)
            {
                var paramGUI = child.GetComponent<IPathInWorld>();
                if(paramGUI != null) path.AddRange(paramGUI.GetWorldPath());
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