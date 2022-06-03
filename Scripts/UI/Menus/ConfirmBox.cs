using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmBox : MonoBehaviour
{
    public Button yesButton;
    public Button noButton;

    public string body;
    public TextMeshProUGUI bodyTmp;
    public CanvasGroup canvasGroup;

    public GameObject panel;
    
    private void Start()
    {
        panel.transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        panel.TweenLocalScale(Vector3.one, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);

        canvasGroup.TweenCanvasGroupAlpha(1f, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);

        bodyTmp.text = body;

        yesButton.onClick.AddListener(Exit);
        noButton.onClick.AddListener(Exit);
    }

    private void Exit()
    {
        canvasGroup.TweenCanvasGroupAlpha(0f, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);
        
        panel.TweenLocalScale(Vector3.zero, 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true)
            .SetOnComplete(() => Destroy(gameObject));
    }
}
