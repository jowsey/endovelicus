using System;
using System.Collections.Generic;
using System.Linq;
using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Random = System.Random;

public class UpgradePanel : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    public TextMeshProUGUI deityPointCounter;
    public GameObject upgradeOptionsContainer;
    public Button confirmButton;

    [NonSerialized]
    public int temporaryDeityPoints;

    public readonly List<Upgrade> upgradesChosen = new List<Upgrade>();

    // Start is called before the first frame update
    private void Start()
    {
        transform.localScale = Vector3.zero;
        transform.TweenLocalScale(Vector3.one, 0.25f).SetEaseCubicInOut().SetUseUnscaledTime(true);

        titleText.text = $"You reached Deity Level {LevelManager.instance.deityLevel}!";

        UIManager.instance.spellUI.ToggleSpellUsage(false);
        TimeManager.instance.ToggleTimeControls(false);
        TimeManager.instance.pauseButton.onClick.Invoke();

        transform.SetSiblingIndex(transform.parent.childCount - 2);
        UpdateTemporaryDeityPoints();

        // Take up to 3 random available upgrades
        var rnd = new Random();
        var upgradeSelection = Upgrades.AvailableUpgrades.OrderBy(_ => rnd.Next()).Take(3).ToList();

        foreach (var upgrade in upgradeSelection)
        {
            var upgradeOption = Addressables.InstantiateAsync("Prefabs/UI/UpgradeOption", upgradeOptionsContainer.transform).WaitForCompletion().GetComponent<UpgradeOption>();
            upgradeOption.upgrade = upgrade;
        }

        confirmButton.onClick.AddListener(() =>
        {
            LevelManager.instance.deityPoints -= upgradesChosen.Count;
            foreach (var upgrade in upgradesChosen)
            {
                Upgrades.RedeemUpgrade(upgrade);
                StatsManager.stats[StatType.UpgradesBought]++;
            }

            transform.TweenLocalScaleY(0f, 0.25f).SetEaseCubicIn().SetUseUnscaledTime(true).SetOnComplete(() =>
            {
                UIManager.instance.spellUI.ToggleSpellUsage(true);
                TimeManager.instance.ToggleTimeControls(true);
                TimeManager.instance.playButton.onClick.Invoke();
                Destroy(gameObject);
            });
        });
    }

    public void UpdateTemporaryDeityPoints()
    {
        temporaryDeityPoints = LevelManager.instance.deityPoints - upgradesChosen.Count;
        deityPointCounter.text = temporaryDeityPoints.ToString();

        // Red if none left, yellow if some left, green if none used
        if (temporaryDeityPoints == 0)
            deityPointCounter.text = "<color=#FFC083>" + temporaryDeityPoints;
        else if (temporaryDeityPoints < LevelManager.instance.deityPoints)
            deityPointCounter.text = "<color=#FFE2A5>" + temporaryDeityPoints;
        else
            deityPointCounter.text = "<color=#E2FFA5>" + temporaryDeityPoints;
    }
}
