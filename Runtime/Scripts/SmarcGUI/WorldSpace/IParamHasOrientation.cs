using SmarcGUI.MissionPlanning.Params;
using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public interface IParamHasOrientation
    {
        public Orientation GetROSOrientation();
        public Quaternion GetUnityQuaternion();
        public void SetROSOrientation(Orientation o);
        public void SetUnityQuaternion(Quaternion q);
    }
}