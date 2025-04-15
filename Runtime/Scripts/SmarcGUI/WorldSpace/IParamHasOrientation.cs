using SmarcGUI.MissionPlanning.Params;

namespace SmarcGUI.WorldSpace
{
    public interface IParamHasOrientation
    {
        public Orientation GetOrientation();
        public void SetOrientation(Orientation orientation);
    }
}