using System.Collections;
using System.Collections.Generic;
using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeOption : MonoBehaviour
{
    public Upgrade upgrade;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Button selectButton;
    public Outline outline;

    private bool selected;
    public UpgradePanel upgradePanel;

    // Start is called before the first frame update
    private void Start()
    {
        upgradePanel = GetComponentInParent<UpgradePanel>();

        iconImage.sprite = upgrade.icon;
        nameText.text = upgrade.name;
        descriptionText.text = upgrade.description;

        selectButton.onClick.AddListener(() =>
        {
            if(upgradePanel.temporaryDeityPoints == 0 && !selected) return;

            selected = !selected;
            outline.enabled = selected;

            gameObject.TweenLocalPositionY(selected ? 8f : 0, 0.1f).SetEaseCubicInOut().SetUseUnscaledTime(true);

            if(selected)
            {
                upgradePanel.upgradesChosen.Add(upgrade);
            }
            else
            {
                upgradePanel.upgradesChosen.Remove(upgrade);
            }

            upgradePanel.UpdateTemporaryDeityPoints();
        });
    }
}
