using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI bodyText;

    private LayoutElement layoutElement;
    private RectTransform rectTransform;

    public TooltipTrigger trigger;

    private void UpdateContent()
    {
        // If nothing has changed, no need to calculate anything new
        if (headerText.text == trigger.header && bodyText.text == trigger.body) return;

        headerText.text = trigger.header;
        bodyText.text = trigger.body;

        // Only enable text if there is content
        headerText.gameObject.SetActive(!string.IsNullOrEmpty(trigger.header));
        bodyText.gameObject.SetActive(!string.IsNullOrEmpty(trigger.body));

        // If the text is wider than the tooltip's preffered (max) width, clamp the width
        layoutElement.enabled = Mathf.Max(headerText.preferredWidth, bodyText.preferredWidth) > layoutElement.preferredWidth;
    }

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Update position every frame
    private void LateUpdate()
    {
        // Pivots the tooltip away from the screen edge to ensure it's always visible
        var pivotX = Input.mousePosition.x < Screen.width / 2f ? 0 : 1;
        var pivotY = Input.mousePosition.y < Screen.height / 2f ? 0 : 1;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = Input.mousePosition;

        UpdateContent();
    }
}
