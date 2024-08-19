using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour
{
    [SerializeField] private Slider valueSlider;
    [SerializeField] private Text valueText;
    [SerializeField] private float maxValue = 5f;
    [SerializeField] private float minValue = -5f;

    [SerializeField] private Image fillImage;

    public void SetSliderValue(float value)
    {
        var normalizedValue = (value - minValue) / (maxValue - minValue);
        valueSlider.value = normalizedValue;
        fillImage.color = Color.Lerp(Color.red, Color.green, normalizedValue);
    }
}
