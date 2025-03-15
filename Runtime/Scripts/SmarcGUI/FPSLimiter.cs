using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class FPSLimiter : MonoBehaviour 
{
    public Slider slider;
    public TMP_Text text;

	void Start()
	{
        slider.onValueChanged.AddListener(OnSliderValueChanged);
		QualitySettings.vSyncCount = 0;
        OnSliderValueChanged(slider.value);
	}

    void OnSliderValueChanged(float value)
    {
        Application.targetFrameRate = (int)value;
        text.text = Application.targetFrameRate.ToString();
    }


}