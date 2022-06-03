using System.Linq;
using ElRaccoone.Tweens;
using UnityEngine;
using UnityEngine.UI;

public class VillageOrder : MonoBehaviour
{
    [SerializeField]
    private Image border;

    public SummonOrder order;

    public void Start()
    {
        var rt = GetComponent<RectTransform>();
        var originalSize = rt.sizeDelta.x;
        rt.sizeDelta = Vector2.zero;

        // Animate in
        gameObject.TweenValueFloat(originalSize, 0.25f, val => rt.sizeDelta = new Vector2(val, val))
            .SetEaseCubicIn().SetOnComplete(() =>
            {
                var totalUnits = order.unitSets.Sum(u => u.Value);

                gameObject.TweenValueFloat(1f, totalUnits * 0.5f, val => border.fillAmount = val).SetOnComplete(() =>
                {
                    // Begin summoning units
                    StartCoroutine(Summon.SummonUnits(order));

                    // Animate order border (heh) to be the team's colour
                    border.rectTransform.TweenGraphicColor(order.ownership == UnitOwnership.Friendly ? Constants.friendlyColour : Constants.enemyColour, 0.5f).SetEaseCubicIn();

                    // When all units are summoned, remove the order
                    order.onComplete.AddListener(() =>
                    {
                        // Animate out and destroy
                        gameObject.TweenValueFloat(0f, 0.25f, val => rt.sizeDelta = new Vector2(val, val))
                            .SetFrom(originalSize)
                            .SetEaseCubicOut()
                            .SetOnComplete(() => Destroy(gameObject));
                    });
                });
            });

    }
}
