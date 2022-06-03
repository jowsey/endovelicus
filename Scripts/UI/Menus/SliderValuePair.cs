using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SliderValuePair : MonoBehaviour
{
    private Slider slider;
    private TextMeshProUGUI text;

    public void Start()
    {
        slider = GetComponentInChildren<Slider>();
        text = GetComponentsInChildren<TextMeshProUGUI>().First(t => t.name == "Value");
    }

    public void Update()
    {
        text.text = Mathf.Approximately(slider.value, Mathf.Round(slider.value)) ? $"{slider.value}" : $"{slider.value:0.0}";
    }
}
