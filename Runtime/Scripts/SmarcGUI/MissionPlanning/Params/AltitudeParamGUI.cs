using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;

namespace SmarcGUI.MissionPlanning.Params
{
    public class AltitudeParamGUI : ParamGUI, IParamHasY
    {
        [Header("AltitudeParamGUI")]
        public TMP_InputField AltitudeField;

        public float altitude
        {
            get { return ((Altitude)paramValue).altitude; }
            set
            {
                var d = (Altitude)paramValue;
                d.altitude = value;
                paramValue = d;
                AltitudeField.text = value.ToString();
                NotifyPathChange();
            }
        }

        protected override void SetupFields()
        {
            altitude = 10;
            AltitudeField.text = altitude.ToString();
            AltitudeField.onEndEdit.AddListener(value => SetY(float.Parse(value)));
            fields.Add(AltitudeField.GetComponent<RectTransform>());
        }

        public float GetY()
        {
            return altitude;
        }

        public float GetYReference()
        {
            return 0;
        }

        public void SetY(float y)
        {
            altitude = y;
        }
    }
}
