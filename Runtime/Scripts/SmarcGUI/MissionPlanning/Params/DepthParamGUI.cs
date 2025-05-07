using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;

namespace SmarcGUI.MissionPlanning.Params
{
    public class DepthParamGUI : ParamGUI, IParamHasY
    {
        [Header("DepthParamGUI")]
        public TMP_InputField DepthField;

        public float depth
        {
            get { return ((Depth)paramValue).depth; }
            set
            {
                var d = (Depth)paramValue;
                d.depth = value;
                paramValue = d;
                DepthField.text = value.ToString();
                NotifyPathChange();
            }
        }

        protected override void SetupFields()
        {
            depth = -1;
            DepthField.text = depth.ToString();
            DepthField.onEndEdit.AddListener(value => SetY(-float.Parse(value)));
            fields.Add(DepthField);
        }

        public float GetY()
        {
            return -depth;
        }

        public float GetYReference()
        {
            return 0;
        }

        public void SetY(float y)
        {
            depth = -y;
        }
    }
}
