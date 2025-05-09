using System.Collections;
using System.Collections.Generic;
using SmarcGUI.MissionPlanning.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmarcGUI.MissionPlanning.Params
{
    public class ParamGUI : MonoBehaviour, IListItem, IHeightUpdatable
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

        protected RectTransform rt;

        protected List<RectTransform> fields = new();
 
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

        void Awake()
        {
            missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            guiState = FindFirstObjectByType<GUIState>();
            rt = GetComponent<RectTransform>();
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
            ReArrangeForList();
        }

        void UpdateLabel()
        {
            if(Label == null) return;
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

        public List<RectTransform> GetFields()
        {
            return fields;
        }

        public virtual List<string> GetFieldLabels()
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

        public void UpdateHeight()
        {
            float selfHeight = 5;
            foreach(Transform child in transform)
            {
                if(child.gameObject.activeSelf)
                    selfHeight += child.GetComponent<RectTransform>().sizeDelta.y;
            }
            
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, selfHeight);
        }

        void ReArrangeForList()
        {
            if(listParamGUI == null) return;
            // we want to re-arrange all the fields in of this param
            // into one line, without any labels, without changing their sizes.
            // this is because the list param gui will take care of the labels for _all of the items_.
            // so, first move all the fields to one common parent
            GameObject fieldsParent = new("Fields");
            fieldsParent.transform.SetParent(transform);
            var fieldsRT = fieldsParent.AddComponent<RectTransform>();
            var fieldsLayout = fieldsParent.AddComponent<HorizontalLayoutGroup>();
            fieldsLayout.childControlHeight = false;
            fieldsLayout.childControlWidth = false;
            fieldsLayout.childForceExpandHeight = false;
            fieldsLayout.childForceExpandWidth = false;
            fieldsLayout.childScaleHeight = false;
            fieldsLayout.childScaleWidth = false;
            fieldsLayout.childAlignment = TextAnchor.MiddleLeft;
            fieldsLayout.spacing = 2;
            fieldsLayout.padding = new RectOffset(2, 2, 0, 0);

            // then, move all the fields to this new parent
            float totalChildrenWidth = 0;
            float maxChildHeight = 0;
            foreach(var field in fields)
            {
                field.transform.SetParent(fieldsParent.transform);
                totalChildrenWidth += field.sizeDelta.x;
                if(field.sizeDelta.y > maxChildHeight)
                    maxChildHeight = field.sizeDelta.y;
            }
            
            if(rt == null) rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, maxChildHeight);
            if(totalChildrenWidth > rt.sizeDelta.x)
            {
                var diff = totalChildrenWidth - rt.sizeDelta.x;
                var per = diff / fields.Count;
                foreach(var field in fields)
                {
                    field.sizeDelta = new Vector2(field.sizeDelta.x - per, field.sizeDelta.y);
                }
                totalChildrenWidth -= diff;
            }
            // set the size of the new parent to be the sum of all the children widths, and the max height of the children
            fieldsRT.sizeDelta = new Vector2(totalChildrenWidth, maxChildHeight);
            fieldsRT.anchoredPosition = Vector2.zero;

            // disable all other children of this object
            foreach(Transform child in transform)
            {
                if(child != fieldsParent.transform)
                    child.gameObject.SetActive(false);
            }

        }
    }
}