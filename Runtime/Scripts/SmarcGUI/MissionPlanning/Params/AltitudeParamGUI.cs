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
        }

        public float GetY()
        {
            return altitude;
        }

        public void SetY(float y)
        {
            altitude = y;
        }
    }
}
