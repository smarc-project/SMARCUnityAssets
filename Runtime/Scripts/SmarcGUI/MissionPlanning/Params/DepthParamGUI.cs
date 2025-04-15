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
        }

        public float GetY()
        {
            return -depth;
        }

        public void SetY(float y)
        {
            depth = -y;
        }
    }
}
