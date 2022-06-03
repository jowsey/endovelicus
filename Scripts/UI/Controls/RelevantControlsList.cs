using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class ControlTips
{
    public const string UnitControls = @"Right-click to attack-move units
Ctrl + right-click to direct-move units
Left-click to deselect units
Shift + drag to select another group";

    public const string CastLocationalSpell = "Left-click to cast spell at location";
    public const string ChooseTowerPosition = "Left-click to build tower at location";
    public const string CancelTowerPlacement = "Right-click to cancel building tower";
}

public class RelevantControlsList : MonoBehaviour
{
    public static RelevantControlsList instance { get; private set; }
    public TextMeshProUGUI text;
    public List<string> enabledControls = new List<string>();
    public RectTransform rt;

    public void Add(string s)
    {
        if (enabledControls.Contains(s)) return;

        enabledControls.Add(s);
        UpdateText();
    }

    public void Remove(string s)
    {
        if (!enabledControls.Contains(s)) return;

        enabledControls.Remove(s);
        UpdateText();
    }

    private void UpdateText()
    {
        text.text = string.Join("\n", enabledControls.ToArray());
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        rt.localScale = text.rectTransform.sizeDelta.y == 0 ? Vector3.zero : Vector3.one;
    }

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        rt = GetComponent<RectTransform>();
    }
}
