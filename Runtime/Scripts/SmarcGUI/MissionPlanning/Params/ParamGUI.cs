using System.Collections;
using SmarcGUI.MissionPlanning.Tasks;
using TMPro;
using UnityEngine;

namespace SmarcGUI.MissionPlanning.Params
{
    public class ParamGUI : MonoBehaviour, IListItem
    {
        [Header("ParamGUI")]
        public TMP_Text Label;
        
        protected IDictionary paramsDict;
        public string ParamKey{get; protected set;}
        protected TaskGUI taskgui;

        protected IList paramsList;
        public int ParamIndex{get; protected set;}
        protected ListParamGUI listParamGUI;
        
        protected MissionPlanStore missionPlanStore;
        protected GUIState guiState;
 

        void Awake()
        {
            missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            guiState = FindFirstObjectByType<GUIState>();
        }

        public object paramValue
        {
            get => paramsDict!=null? paramsDict[ParamKey] : paramsList[ParamIndex];
            protected set
            {
                if(paramsDict!=null)
                    paramsDict[ParamKey] = value;
                else
                    paramsList[ParamIndex] = value;
            }
        }

        public void SetParam(IDictionary paramsDict, string paramKey, TaskGUI taskgui)
        {
            this.paramsDict = paramsDict;
            this.ParamKey = paramKey;
            this.taskgui = taskgui;
            UpdateLabel();
            SetupFields();
        }
        public void SetParam(IList paramsList, int paramIndex, ListParamGUI listParamGUI)
        {   
            this.paramsList = paramsList;
            this.ParamIndex = paramIndex;
            this.listParamGUI = listParamGUI;
            UpdateLabel();
            SetupFields();
        }

        void UpdateLabel()
        {
            Label.text = ParamKey ?? ParamIndex.ToString();
        }

        public void UpdateIndex(int newIndex)
        {
            ParamIndex = newIndex;
            UpdateLabel();
        }

        protected virtual void SetupFields()
        {
            throw new System.NotImplementedException();
        }

        protected void NotifyPathChange()
        {
            if(taskgui != null) taskgui.OnParamChanged();
            if(listParamGUI != null) listParamGUI.OnParamChanged();
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