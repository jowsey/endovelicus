using System.Collections;
using System.Collections.Generic;
using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour
{
    public TooltipTrigger tooltipTrigger;

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI label;
    public Image icon;
    public TowerData towerData;

    // Start is called before the first frame update
    private void Start()
    {
        icon.sprite = towerData.Icon;

        UpdateText();

        GetComponent<Button>().onClick.AddListener(() => StartCoroutine(Build()));
    }

    private IEnumerator Build()
    {
        // Get resource counts
        var gold = UIManager.instance.inventory.GetStatistic("gold");
        var devotion = UIManager.instance.inventory.GetStatistic("devotion");

        // If not sufficient resources, return
        if (gold.Count < towerData.GetGoldCost() || devotion.Count < towerData.GetDevotionCost()) yield break;

        // Deduct resources
        gold.Count -= towerData.GetGoldCost();
        devotion.Count -= towerData.GetDevotionCost();

        // Animate transparency of button and countdown timer
        canvasGroup.TweenCanvasGroupAlpha(0.33f, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);

        // Create placeholder tower that follows cursor, snapped to tile grid
        yield return TowerData.GetMouseClickGridPosition(towerData, gridPosition =>
        {
            canvasGroup.TweenCanvasGroupAlpha(1f, 0.25f)
                .SetEaseCubicInOut()
                .SetUseUnscaledTime(true);

            if (gridPosition == Vector3Int.zero)
            {
                gold.Count += towerData.GetGoldCost();
                devotion.Count += towerData.GetDevotionCost();

                return;
            }

            StatsManager.stats[StatType.TowersBuilt]++;
            var tower = Addressables.InstantiateAsync("Prefabs/Towers/Tower")
                .WaitForCompletion()
                .GetComponent<Tower>();

            tower.transform.position = gridPosition + new Vector3Int(1, 1, 0);
            tower.towerData = towerData;

            tower.reservedAreas = new List<Vector3Int>
            {
                gridPosition,
                gridPosition + new Vector3Int(1, 0, 0),
                gridPosition + new Vector3Int(0, 1, 0),
                gridPosition + new Vector3Int(1, 1, 0)
            };

            MapManager.instance.globalReservedAreas.AddRange(tower.reservedAreas);
        });
    }

    private void UpdateText()
    {
        tooltipTrigger.header = "Tower: " + towerData.Name;
        label.text = towerData.Name;

        tooltipTrigger.body = towerData.GetDescription() + "\n\n" +
                              $"{(towerData.GetDevotionCost() > 0 ? $"<color=#5D9AC8>{towerData.GetDevotionCost()} devotion" : "<color=#5D9AC8AA>Free")}</color> - " +
                              $"{(towerData.GetGoldCost() > 0 ? $"<color=#C8AD5D>{towerData.GetGoldCost()} gold" : "<color=#C8AD5DAA>Free")}</color>";
    }
}
