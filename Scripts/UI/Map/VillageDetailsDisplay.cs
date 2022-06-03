using System.Linq;
using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageDetailsDisplay : MonoBehaviour
{
    public Village village;

    public Image progressFill;
    public TextMeshProUGUI detailsText;

    public GameObject ordersContainer;

    public CanvasGroup canvasGroup;

    private const string villageDetailsTemplate = "{ownership} Village ({takeoverProgress}%)\n<size=65%>{captureRequired}+{houseCount}g, +{churchCount}d</size>";

    private void UpdateDetailsText()
    {
        detailsText.text = villageDetailsTemplate
            .Replace("{ownership}", village.ownership.ToString())
            .Replace("{takeoverProgress}", Mathf.Abs(village.takeoverProgress).ToString("0"))
            .Replace("{captureRequired}", village.ownership != Ownership.Friendly ? "Capture for " : "Giving ")
            .Replace("{houseCount}", village.buildings.Count(b => b.type == BuildingType.House).ToString())
            .Replace("{churchCount}", village.buildings.Count(b => b.type == BuildingType.Church).ToString());

        // Background circle around the icon fills based on takeover progress
        progressFill.fillAmount = Mathf.Abs(village.takeoverProgress) / 100f;

        progressFill.color = village.takeoverProgress > 0 ? Constants.friendlyColour : Constants.enemyColour;
        progressFill.fillClockwise = village.takeoverProgress > 0;
    }

    private void Start()
    {
        village.onTakeoverProgressChanged.AddListener(UpdateDetailsText);
        UpdateDetailsText();

        FogManager.instance.onFogUpdate.AddListener(CheckFogVisibility);
        UpdatePosition();
        CheckFogVisibility();
    }

    private void LateUpdate() => UpdatePosition();

    private void CheckFogVisibility()
    {
        // Turn transparent if under fog
        var x = UIManager.instance.cam.ScreenToWorldPoint(transform.position).x;
        canvasGroup.TweenCanvasGroupAlpha(x < FogManager.instance.fogBeginX ? 1f : 0f, 0.25f).SetEaseCubicIn();
    }

    private void UpdatePosition()
    {
        // Set to be over the village
        var screenPos = UIManager.instance.cam.WorldToScreenPoint(village.centerPosition);
        transform.position = screenPos;
    }
}
