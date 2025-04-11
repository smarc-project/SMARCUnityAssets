using TMPro;
using UnityEngine;


namespace SmarcGUI.MissionPlanning.Params
{
    public class OrientationParamGUI : ParamGUI
    {
        public TMP_InputField qwField, qxField, qyField, qzField;
        public TMP_InputField xField, yField, zField;
        


        void UpdateQuatFromEuler()
        {
            var x = xField.text != "" ? float.Parse(xField.text) : 0;
            var y = yField.text != "" ? float.Parse(yField.text) : 0;
            var z = zField.text != "" ? float.Parse(zField.text) : 0;
            var q = Quaternion.Euler(x, y, z);
            qwField.text = q.w.ToString();
            qxField.text = q.x.ToString();
            qyField.text = q.y.ToString();
            qzField.text = q.z.ToString();
            var v = (Orientation)paramValue;
            v.w = q.w;
            v.x = q.x;
            v.y = q.y;
            v.z = q.z;
            paramValue = v;
            NotifyPathChange();
        }

        void NotifyPathChange()
        {
            if(taskgui != null) taskgui.OnParamChanged();
            if(listParamGUI != null) listParamGUI.OnParamChanged();
        }

        void Awake()
        {
            guiState = FindFirstObjectByType<GUIState>();
            qwField.interactable = false;
            qxField.interactable = false;
            qyField.interactable = false;
            qzField.interactable = false;
        }

        protected override void SetupFields()
        {
            UpdateTexts();
            xField.onValueChanged.AddListener(OnEulerXChanged);
            yField.onValueChanged.AddListener(OnEulerYChanged);
            zField.onValueChanged.AddListener(OnEulerZChanged);

            OnSelectedChange();
        }

        void UpdateTexts()
        {
            var v = (Orientation)paramValue;
            qwField.text = v.w.ToString();
            qxField.text = v.x.ToString();
            qyField.text = v.y.ToString();
            qzField.text = v.z.ToString();

            var q = new Quaternion(v.x, v.y, v.z, v.w);
            var euler = q.eulerAngles;
            xField.text = euler.x.ToString();
            yField.text = euler.y.ToString();
            zField.text = euler.z.ToString();
        }


        void OnEulerXChanged(string s)
        {
            try {UpdateQuatFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler X value");
                OnEulerXChanged("0");
                return;
            }
        }

        void OnEulerYChanged(string s)
        {
            try {UpdateQuatFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler Y value");
                OnEulerYChanged("0");
                return;
            }
        }

        void OnEulerZChanged(string s)
        {
            try {UpdateQuatFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler Z value");
                OnEulerZChanged("0");
                return;
            }
        }
    }
}
