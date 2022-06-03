using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ElRaccoone.Tweens;
using ElRaccoone.Tweens.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Int32;

[RequireComponent(typeof(Target))]
public class Tower : MonoBehaviour
{
    [Header("Visual")]
    public LineRenderer borderRenderer;

    public SpriteRenderer sr;

    [Header("AI")]
    public TowerUnit unit;

    public Target target;

    [Header("UI")]
    public Canvas canvas;

    public CanvasGroup canvasGroup;

    public RectTransform healthBar;
    public TextMeshProUGUI healthTmp;

    public Button upgradeButton;
    public TooltipTrigger upgradeTooltip;
    public Button repairButton;
    public TooltipTrigger repairTooltip;

    [Header("Other")]
    public int level = 1;

    public TowerData towerData;

    public const UnitOwnership ownership = UnitOwnership.Friendly;
    public ITween hideHealthBarTween;

    private float _health;

    public float health
    {
        get => _health;
        set
        {
            value = Mathf.Clamp(value, 0, maxHealth);
            if(value == 0)
            {
                Die();
            }

            healthBar.parent.gameObject.SetActive(true);
            healthTmp.text = value.ToString("0");
            healthBar.sizeDelta = new Vector2(1.98f * value / maxHealth, healthBar.sizeDelta.y);

            StartCoroutine(HideHealthBar());

            _health = value;
            UpdateTooltips();
        }
    }

    public int maxHealth => (int) towerData.LevelValue("maxHealth", level);

    // we keep track of which areas the tower has claimed, so it can un-claim them when destroyed
    public List<Vector3Int> reservedAreas = new List<Vector3Int>();

    private IEnumerator HideHealthBar()
    {
        hideHealthBarTween?.Cancel();
        yield return new WaitForSeconds(5f);

        healthBar.parent.gameObject.SetActive(false);
    }

    private void Start()
    {
        health = maxHealth;
        name = $"{towerData.Name} Tower";

        target.tower = this;

        unit.unitClass = towerData.UnitClass;

        UpdateBorder();
        canvasGroup.alpha = 0;

        MapManager.instance.nms.BuildNavMesh();

        canvas.worldCamera = UIManager.instance.cam;

        upgradeButton.onClick.AddListener(() =>
        {
            var cost = (int) towerData.LevelValue("goldCost", level + 1);
            var gold = UIManager.instance.inventory.GetStatistic("gold");

            if(gold.Count < cost) return;

            gold.Count -= cost;
            
            SoundManager.Play(SoundManager.GetAudioClip("SFX/ArrowHit"), 0.75f);
            Upgrade();
        });

        repairButton.onClick.AddListener(() =>
        {
            var repairPrice = (int) ((maxHealth - health) * 0.5);
            if(repairPrice == 0) return;

            var gold = UIManager.instance.inventory.GetStatistic("gold");
            if(gold.Count >= repairPrice)
            {
                gold.Count -= repairPrice;

                SoundManager.Play(SoundManager.GetAudioClip("SFX/SwordHit"), 0.75f);
                Repair();
            }
        });

        UpdateTooltips();
    }

    public void Upgrade()
    {
        var originalMaxHealth = maxHealth;
        level++;
        health += maxHealth - originalMaxHealth;

        UpdateBorder();
        UpdateTooltips();

        // advanced colour theory right here
        // they used to call me "El Artista" back in school
        gameObject.TweenSpriteRendererColor(Color.Lerp(Color.white, Constants.towerGoldColor, ((float) level / towerData.MaxLevel) * 0.7f), 0.15f).SetEaseCubicInOut();

        if(level != towerData.MaxLevel) return;

        upgradeButton.interactable = false;

        var cg = upgradeButton.GetComponent<CanvasGroup>();
        cg.TweenCanvasGroupAlpha(0.5f, 0.25f).SetEaseCubicInOut();
    }

    public void Repair()
    {
        var cg = repairButton.GetComponent<CanvasGroup>();

        repairButton.interactable = false;
        cg.TweenCanvasGroupAlpha(0.5f, 0.25f).SetEaseCubicInOut();

        gameObject.TweenValueFloat(maxHealth, 1f, val => health = val)
            .SetFrom(health)
            .SetEaseCubicInOut()
            .SetOnComplete(() =>
            {
                repairButton.interactable = true;
                cg.TweenCanvasGroupAlpha(1f, 0.25f).SetEaseCubicInOut();
            });
    }

    public void OnMouseEnter()
    {
        canvasGroup.alpha = 1;
        healthBar.parent.gameObject.SetActive(true);

        borderRenderer.enabled = true;
    }

    public void OnMouseExit()
    {
        canvasGroup.alpha = 0;

        healthBar.parent.gameObject.SetActive(false);
        hideHealthBarTween?.Cancel();

        borderRenderer.enabled = false;
    }

    private void UpdateTooltips()
    {
        if(health == maxHealth)
        {
            repairTooltip.header = "";
            repairTooltip.body = "This tower is at full health, and does not need repairs.";
        }
        else
        {
            repairTooltip.header = "Repair tower";
            repairTooltip.body = $"Repair this tower for {(int) ((maxHealth - health) * 0.5)} gold, which will return it to full health.";
        }

        upgradeTooltip.header = level == towerData.MaxLevel ? "" : "Upgrade tower";
        upgradeTooltip.body = level == towerData.MaxLevel
            ? "This tower is at the max level and cannot be upgraded any further."
            : $"Upgrade this tower for {towerData.LevelValue("goldCost", level + 1)} gold, to greatly increase its overall stats.";
    }

    private Tween<float> TweenBorderAlpha(float to)
    {
        return borderRenderer.TweenValueFloat(to, 0.15f, val =>
        {
            var cg = borderRenderer.colorGradient;
            cg.alphaKeys = cg.alphaKeys.Select(key => new GradientAlphaKey(val, key.time)).ToArray();
            borderRenderer.colorGradient = cg;
        }).SetEaseCubicIn().SetFrom(borderRenderer.colorGradient.alphaKeys[0].alpha);
    }

    private void UpdateBorder()
    {
        var radius = towerData.LevelValue("attackRange", level);

        borderRenderer.TweenValueFloat(radius, 0.25f, val =>
        {
            // Set the positions of the border renderer
            for (var i = 0; i < borderRenderer.positionCount; i++)
            {
                var angle = i * Mathf.PI * 2 / borderRenderer.positionCount;
                borderRenderer.SetPosition(i, new Vector3(
                        Mathf.Cos(angle) * val,
                        Mathf.Sin(angle) * val,
                        0
                    )
                );
            }
        }).SetFrom(level > 1 ? towerData.LevelValue("attackRange", level - 1) : 0).SetEaseCubicInOut();
    }

    private void Die()
    {
        foreach (var area in reservedAreas)
        {
            MapManager.instance.globalReservedAreas.Remove(area);
        }

        Destroy(gameObject);
    }
}
