using System.Collections;
using SmarcGUI.MissionPlanning.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SmarcGUI.MissionPlanning.Params
{
    public class ParamGUI : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler, IListItem
    {
        public TMP_Text Label;
        
        protected IDictionary paramsDict;
        protected string paramKey;
        protected TaskGUI taskgui;

        protected IList paramsList;
        public int paramIndex{get; protected set;}
        protected ListParamGUI listParamGUI;

        public RectTransform HighlightRT;
        public RectTransform SelectedHighlightRT;
        
        public GameObject ContextMenuPrefab;

        protected MissionPlanStore missionPlanStore;
        protected GUIState guiState;

        protected bool isSelected;
 

        void Awake()
        {
            missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            guiState = FindFirstObjectByType<GUIState>();
        }

        public object paramValue
        {
            get => paramsDict!=null? paramsDict[paramKey] : paramsList[paramIndex];
            protected set
            {
                if(paramsDict!=null)
                    paramsDict[paramKey] = value;
                else
                    paramsList[paramIndex] = value;
            }
        }

        public void SetParam(IDictionary paramsDict, string paramKey, TaskGUI taskgui)
        {
            this.paramsDict = paramsDict;
            this.paramKey = paramKey;
            this.taskgui = taskgui;
            UpdateLabel();
            SetupFields();
        }
        public void SetParam(IList paramsList, int paramIndex, ListParamGUI listParamGUI)
        {   
            this.paramsList = paramsList;
            this.paramIndex = paramIndex;
            this.listParamGUI = listParamGUI;
            UpdateLabel();
            SetupFields();
        }

        void UpdateLabel()
        {
            Label.text = paramKey ?? paramIndex.ToString();
        }

        public void UpdateIndex(int newIndex)
        {
            paramIndex = newIndex;
            UpdateLabel();
        }

        protected virtual void SetupFields()
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                var contextMenuGO = Instantiate(ContextMenuPrefab);
                var contextMenu = contextMenuGO.GetComponent<ListItemContextMenu>();
                contextMenu.SetItem(eventData.position, this);
            }

            if(eventData.button == PointerEventData.InputButton.Left)
            {
                isSelected = !isSelected;
                SelectedHighlightRT?.gameObject.SetActive(isSelected);
                OnSelectedChange();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HighlightRT?.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HighlightRT?.gameObject.SetActive(true);
        }

        protected virtual void OnSelectedChange()
        {
            return;
        }

        public void OnListItemUp()
        {
            listParamGUI.MoveParamUp(this);
        }

        public void OnListItemDown()
        {
            listParamGUI.MoveParamDown(this);
        }

        public void OnListItemDelete()
        {
            listParamGUI.DeleteParam(this);
        }
    }
}