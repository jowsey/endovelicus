using System.Collections;
using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Notification : MonoBehaviour, IPointerClickHandler
{
    public LayoutElement layoutElement;
    public TextMeshProUGUI tmp;
    public Image image;
    public TooltipTrigger tooltipTrigger;

    public Sprite icon;
    public string text;

    private IEnumerator Start()
    {
        image.sprite = icon;
        tmp.text = text;

        // Force-update dimensions now we have updated text
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        // Set max width cap if needed
        // If max width cap is enabled, that implies the text will be cut off, so enable tooltip with full text
        layoutElement.enabled = GetComponent<RectTransform>().rect.width > layoutElement.preferredWidth;

        tooltipTrigger.body = layoutElement.enabled ? text : "";

        // Animate fade in with scale
        transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(0.5f);
        gameObject.TweenLocalScale(Vector3.one, 0.1f).SetEaseCubicIn().SetUseUnscaledTime(true);
    }

    // When mouse is clicked on this notification
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;
        
        // When clicked, animate a little, then destroy
        var rt = GetComponent<RectTransform>();

        gameObject.TweenValueFloat(0f, 0.25f, val => rt.sizeDelta = new Vector2(rt.sizeDelta.x, val))
            .SetFrom(rt.sizeDelta.y)
            .SetEaseCubicIn()
            .SetUseUnscaledTime(true);
            
        gameObject.TweenLocalScaleY(0f, 0.25f)
            .SetEaseCubicIn()
            .SetUseUnscaledTime(true);

        GetComponent<CanvasGroup>().TweenCanvasGroupAlpha(0f, 0.25f)
            .SetEaseCubicIn()
            .SetUseUnscaledTime(true)
            .SetOnComplete(() => Destroy(gameObject));
    }
}
