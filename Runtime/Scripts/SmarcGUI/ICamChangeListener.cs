using UnityEngine;

namespace SmarcGUI
{
    public interface ICamChangeListener
    {
        void OnCamChange(Camera newCam);
    }
}