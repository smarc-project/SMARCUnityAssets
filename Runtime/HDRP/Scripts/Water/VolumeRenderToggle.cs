using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace SmarcGUI.Water
{
    
    [RequireComponent(typeof(Volume))]
    public class VolumeRenderToggle : WaterRenderToggle
    {
        private Volume _volumeSettings;

        void Awake()
        {
            _volumeSettings = GetComponent<Volume>();
        }
        public override void ToggleWaterRender(bool render)
        {
            _volumeSettings.profile.TryGet<WaterRendering>(out var waterRendering);
            waterRendering.enable.value = render;
        }
    }
}