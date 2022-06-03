using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static System.Int32;

public class LevelManager : MonoBehaviour
{
    private int _xp;

    public int xp
    {
        get => _xp;
        set
        {
            while (value >= requiredXp)
            {
                value -= requiredXp;
                deityLevel++;
            }

            _xp = value;

            UpdateVisuals();
        }
    }

    private int requiredXp = 15;

    public int deityPoints;

    private int _deityLevel = 1;

    public int deityLevel
    {
        get => _deityLevel;
        set
        {
            value = Mathf.Clamp(value, 1, MaxValue);

            requiredXp = Mathf.CeilToInt(15 * Mathf.Pow(1.14f, value - 1));

            deityPoints += value - _deityLevel;
            _deityLevel = value;

            Addressables.InstantiateAsync("Prefabs/UI/UpgradePanel", UIManager.instance.canvas.transform).WaitForCompletion();
            UpdateVisuals();
        }
    }

    public static LevelManager instance { get; private set; }

    public Image xpBar;
    public TextMeshProUGUI deityLevelText;
    private RectTransform rt;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        rt = GetComponent<RectTransform>();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        deityLevelText.text = $"Level {deityLevel}   <size=70%>{xp} / {requiredXp}xp</size>";

        xpBar.rectTransform.TweenValueVector2(new Vector2(rt.rect.size.x * ((float)xp / requiredXp), xpBar.rectTransform.rect.size.y), 0.25f, val => xpBar.rectTransform.sizeDelta = val)
            .SetFrom(xpBar.rectTransform.rect.size)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            xp += 5;
        }
#endif
    }

    // update width if screen size or something similar changes
    private void OnRectTransformDimensionsChange()
    {
        // stops from calling before awake, when unity sets the initial window size
        // doesn't matter then anyway, the bar's going to be empty and a frame hasn't even been rendered yet
        if (rt == null) return;
        UpdateVisuals();
    }
}
