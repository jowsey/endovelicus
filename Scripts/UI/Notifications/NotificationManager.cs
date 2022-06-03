using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager instance { get; private set; }
    public TextMeshProUGUI header;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        // If there are no notifications, hide the notification box title
        header.gameObject.SetActive(transform.childCount > 1);
    }

    public void SendNotification(string text, Sprite icon)
    {
        // Create a new notification
        var notification = Addressables.InstantiateAsync("Prefabs/UI/Notification").WaitForCompletion().GetComponent<Notification>();
        notification.GetComponent<RectTransform>().SetParent(transform);

        // Set notification content
        notification.icon = icon;
        notification.text = text;
    }
}