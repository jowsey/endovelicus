using ElRaccoone.Tweens;
using UnityEngine;

// Some code taken or inspired by https://www.youtube.com/watch?v=HXFoUGw7eKk
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance { get; private set; }
    public Tooltip tooltip;

    private void Awake()
    {
        // Singleton pattern
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public static void Show(TooltipTrigger trigger)
    {
        if(string.IsNullOrEmpty(trigger.header) && string.IsNullOrEmpty(trigger.body)) return;

        instance.tooltip.gameObject.SetActive(true);
        instance.tooltip.trigger = trigger;

        // Fade in transparency;
        var canvasGroup = instance.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        
        canvasGroup.TweenCanvasGroupAlpha(1f, 0.1f).SetUseUnscaledTime(true);
    }

    public static void Hide()
    {
        instance.tooltip.gameObject.SetActive(false);
    }
}
