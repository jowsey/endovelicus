using ElRaccoone.Tweens;
using UnityEngine;

public class Border : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Toggle(bool setEnabled)
    {
        if (setEnabled)
        {
            gameObject.TweenCancelAll();

            canvasGroup.TweenCanvasGroupAlpha(1f, 1f)
                .SetOnComplete(() => canvasGroup.TweenCanvasGroupAlpha(0.5f, 1f).SetPingPong().SetInfinite().SetUseUnscaledTime(true))
                .SetUseUnscaledTime(true);
        }
        else
        {
            gameObject.TweenCancelAll();

            canvasGroup.TweenCanvasGroupAlpha(0f, 1f).SetUseUnscaledTime(true);
        }
    }
}
