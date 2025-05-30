namespace SmarcGUI.WorldSpace
{
    public interface IParamHasXZ
    {
        public (float, float) GetXZ();
        public void SetXZ(float x, float z);
    }
}