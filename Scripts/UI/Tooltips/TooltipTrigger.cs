using ElRaccoone.Tweens.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using Task = System.Threading.Tasks.Task;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ITween delay;
    public string header;
    public string body;

    public bool pointerIsOver;

    private async void ShowTooltip()
    {
        pointerIsOver = true;

        // Delay opening for 0.5 seconds. If mouse exits before that time, tooltip won't show.
        await Task.Delay(500);

        if(pointerIsOver)
            TooltipManager.Show(this);
    }

    private void HideTooltip()
    {
        pointerIsOver = false;
        TooltipManager.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData) => ShowTooltip();
    public void OnMouseEnter() => ShowTooltip();

    public void OnPointerExit(PointerEventData eventData) => HideTooltip();
    public void OnMouseExit() => HideTooltip();
}
