using UnityEngine;

namespace SmarcGUI.KeyboardControllers
{
    public abstract class KeyboardControllerBase : MonoBehaviour
    {        
        public abstract void OnReset();

        public void Disable()
        {
            OnReset();
            enabled = false;
        }

        public void Enable()
        {
            OnReset();
            enabled = true;
        }


    }
}