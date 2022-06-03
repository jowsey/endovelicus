using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatisticCounterItem : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI valueText;

    public string Id;

    public TooltipTrigger tooltipTrigger;

    public Sprite Icon
    {
        get => icon.sprite;
        set => icon.sprite = value;
    }

    public int Count
    {
        get => int.Parse(valueText.text);
        set => valueText.text = value.ToString();
    }
}