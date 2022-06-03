using System.Collections;
using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour
{
    public TooltipTrigger tooltipTrigger;

    public CanvasGroup mainCanvasGroup;
    public CanvasGroup countdownTimerCanvasGroup;
    public TextMeshProUGUI countdownTimerText;
    public TextMeshProUGUI label;
    public Image icon;
    public Spell spell;

    // Start is called before the first frame update
    private void Start()
    {
        icon.sprite = spell.Icon;

        spell.onLevelChange.AddListener(UpdateText);
        UpdateText();

        spell.onCooldownEnd.AddListener(CooldownEndTween);

        GetComponent<Button>().onClick.AddListener(() => StartCoroutine(Cast()));
    }

    private IEnumerator Cast()
    {
        // If cooldown hasn't finished, return
        if (Time.time < spell.CooldownEndTime) yield break;

        // Get resource counts
        var gold = UIManager.instance.inventory.GetStatistic("gold");
        var devotion = UIManager.instance.inventory.GetStatistic("devotion");

        // If not sufficient resources, return
        if (gold.Count < spell.GetGoldCost() || devotion.Count < spell.GetDevotionCost()) yield break;

        StatsManager.stats[StatType.SpellsUsed]++;

        // Deduct resources
        gold.Count -= spell.GetGoldCost();
        devotion.Count -= spell.GetDevotionCost();

        // Set new cooldown end time
        spell.CooldownEndTime = Time.time + spell.GetCooldown();

        // Animate transparency of button and countdown timer
        mainCanvasGroup.TweenCanvasGroupAlpha(0.33f, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);

        countdownTimerCanvasGroup.TweenCanvasGroupAlpha(1f, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);

        // Run spell-specific code
        StartCoroutine(spell.CastEffect());

        yield return new WaitForSeconds(spell.GetCooldown());
        spell.onCooldownEnd.Invoke();
    }

    private void CooldownEndTween()
    {
        mainCanvasGroup.TweenCanvasGroupAlpha(1f, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);

        countdownTimerCanvasGroup.TweenCanvasGroupAlpha(0f, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);
    }

    // Update the countdown timer if the spell is on cooldown
    private void LateUpdate()
    {
        if (Time.time < spell.CooldownEndTime) countdownTimerText.text = (spell.CooldownEndTime - Time.time).ToString("0s");
    }

    private void UpdateText()
    {
        tooltipTrigger.header = "Spell: " + spell.LevelledName;
        label.text = spell.LevelledName;

        tooltipTrigger.body = spell.GetDescription() + "\n\n" +
                              $"{(spell.GetDevotionCost() > 0 ? $"<color=#5D9AC8>{spell.GetDevotionCost()} devotion" : "<color=#5D9AC8AA>Free")}</color> - " +
                              $"{(spell.GetGoldCost() > 0 ? $"<color=#C8AD5D>{spell.GetGoldCost()} gold" : "<color=#C8AD5DAA>Free")}</color> - " +
                              $"{(spell.GetCooldown() > 0 ? $"<color=#C7DCD0>{spell.GetCooldown()}s cooldown" : "<color=#C7DCD0AA>Instant")}</color>";
    }
}
