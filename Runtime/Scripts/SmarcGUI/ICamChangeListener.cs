using UnityEngine;

namespace SmarcGUI
{
    public interface ICamChangeListener
    {
        public void OnCamChange(Camera newCam);
    }
}