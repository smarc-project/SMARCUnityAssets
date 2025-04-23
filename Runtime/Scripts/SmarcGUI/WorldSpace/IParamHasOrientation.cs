using SmarcGUI.MissionPlanning.Params;

namespace SmarcGUI.WorldSpace
{
    public interface IParamHasOrientation
    {
        public Orientation GetROSOrientation();
        public Quaternion GetUnityOrientation();
        public void SetROSOrientation(Orientation o);
        public void SetUnityOrientation(Quaternion q);
    }
}