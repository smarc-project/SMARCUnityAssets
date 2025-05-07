using System.Collections;
using System.Collections.Generic;
using SmarcGUI.WorldSpace;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace SmarcGUI.MissionPlanning.Params
{
    public class ListParamGUI : ParamGUI, IPathInWorld, IParamChangeListener //, IHeightUpdatable, 
    {
        [Header("ListParamGUI")]
        // RectTransform rt;

        public RectTransform content;
        public Button AddButton;

        IList paramList => (IList)paramValue;

        // void Awake()
        // {
        //     rt = GetComponent<RectTransform>();
        // }


        protected override void SetupFields()
        {            
            MissionPlanStore missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            for(int i=0; i<((IList)paramValue).Count; i++)
            {
                GameObject paramGO;
                GameObject paramPrefab = missionPlanStore.GetParamPrefab(((IList)paramValue)[i]);
                paramGO = Instantiate(paramPrefab, content);
                paramGO.GetComponent<ParamGUI>().SetParam((IList)paramValue, i, this);
            }

            AddButton.onClick.AddListener(AddParamToList);

            UpdateHeight();
        }

        void AddParamToList()
        {
            if (paramList is null)
                return;

            // Assuming theList contains elements of a specific type, e.g., ParamType
            // if this is not the case, something has gone horribly wrong on the
            // TaskSpecTree side of things.
            // This aint python, lists usually cant contain arbitrary mixes of types
            var paramType = paramList.GetType().GetGenericArguments()[0];
            var newParam = System.Activator.CreateInstance(paramType);

            paramList.Add(newParam);

            // Instantiate the new parameter GUI
            if(missionPlanStore == null) missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            GameObject paramPrefab = missionPlanStore.GetParamPrefab(newParam);
            GameObject paramGO = Instantiate(paramPrefab, content);
            paramGO.GetComponent<ParamGUI>().SetParam(paramList, math.max(0, paramList.Count - 1), this);

            UpdateHeight();
        }

        public void MoveParamUp(ParamGUI paramgui)
        {
            if(paramList == null) return;
            if(paramgui.ParamIndex == 0) return;
            (paramList[paramgui.ParamIndex-1], paramList[paramgui.ParamIndex]) = (paramList[paramgui.ParamIndex], paramList[paramgui.ParamIndex-1]);
            paramgui.transform.SetSiblingIndex(paramgui.ParamIndex - 1);
            paramgui.UpdateIndex(paramgui.ParamIndex - 1);
            paramgui.transform.parent.GetChild(paramgui.ParamIndex+1).GetComponent<ParamGUI>().UpdateIndex(paramgui.ParamIndex+1);
        }
        

        public void MoveParamDown(ParamGUI paramgui)
        {
            if(paramList == null) return;
            if(paramgui.ParamIndex == paramList.Count-1) return;
            (paramList[paramgui.ParamIndex+1], paramList[paramgui.ParamIndex]) = (paramList[paramgui.ParamIndex], paramList[paramgui.ParamIndex+1]);
            paramgui.transform.SetSiblingIndex(paramgui.ParamIndex + 1);
            paramgui.UpdateIndex(paramgui.ParamIndex + 1);
            paramgui.transform.parent.GetChild(paramgui.ParamIndex-1).GetComponent<ParamGUI>().UpdateIndex(paramgui.ParamIndex-1);
        }

        public void DeleteParam(ParamGUI paramgui)
        {
            if(paramList == null) return;
            var originalIndex = paramgui.ParamIndex;
            paramList.RemoveAt(paramgui.ParamIndex);
            Destroy(paramgui.gameObject);
            // Update the indices of the remaining parameters that was originally below deleted one
            for(int i=originalIndex; i<paramgui.transform.parent.childCount; i++)
                paramgui.transform.parent.GetChild(i).GetComponent<ParamGUI>().UpdateIndex(i-1);
            UpdateHeight();
            transform.parent.GetComponentInParent<IHeightUpdatable>()?.UpdateHeight();
        }


        // public void UpdateHeight()
        // {
        //     float contentHeight = 5;
        //     foreach(Transform child in content)
        //         contentHeight += child.GetComponent<RectTransform>().sizeDelta.y;
        //     content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
            
        //     float selfHeight = 5;
        //     foreach(Transform child in transform)
        //         selfHeight += child.GetComponent<RectTransform>().sizeDelta.y;
            
        //     // can happen if someone (like load missions) calls this before awake lol.
        //     if(rt == null) rt = GetComponent<RectTransform>();

        //     rt.sizeDelta = new Vector2(rt.sizeDelta.x, selfHeight);

        //     transform.parent.GetComponentInParent<IHeightUpdatable>()?.UpdateHeight();
        // }

        void OnDisable()
        {
            foreach (Transform child in content)
            {
                child.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            foreach (Transform child in content)
            {
                child.gameObject.SetActive(true);
            }
            UpdateHeight();
        }

        public List<Vector3> GetWorldPath()
        {
            List<Vector3> path = new();
            foreach(Transform child in content)
            {
                if(child.TryGetComponent<IPathInWorld>(out var paramGUI)) path.AddRange(paramGUI.GetWorldPath());
            }
            return path;
        }

        public void OnParamChanged()
        {
            if(taskgui != null) taskgui.OnParamChanged();
            if(listParamGUI != null) listParamGUI.OnParamChanged();
        }
    }
}