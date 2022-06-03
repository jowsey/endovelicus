using UnityEngine;
using UnityEngine.UI;

public class AreaTakeoverVillageIcon : MonoBehaviour
{
    public Village village;
    public Image progressFill;

    private void Start()
    {
        village.onTakeoverProgressChanged.AddListener(UpdateIcon);
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        // Background circle around the icon fills based on takeover progress
        progressFill.fillAmount = Mathf.Abs(village.takeoverProgress) / 100f;

        progressFill.color = village.takeoverProgress > 0 ? Constants.friendlyColour : Constants.enemyColour;
        progressFill.fillClockwise = village.takeoverProgress > 0;
    }
}
