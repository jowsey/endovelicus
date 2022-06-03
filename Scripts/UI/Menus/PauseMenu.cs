using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElRaccoone.Tweens;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private List<Button> menuOptions;
    public CanvasGroup canvasGroup;
    public GameObject menu;
    public static bool isPaused = false;

    private void SelectButton(GameObject button)
    {
        UnselectAllButtons();
        var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        tmp.color = new Color(244 / 255f, 173 / 255f, 82 / 255f, 1f);
        tmp.text += " <color=white><</color>";
    }

    private void UnselectAllButtons()
    {
        // Remove all select effects from each button (we go through every button to cover edge cases when using both mouse and arrow keys)
        foreach (var tmp in menuOptions.Select(button => button.GetComponentInChildren<TextMeshProUGUI>()))
        {
            tmp.color = Color.white;
            tmp.text = tmp.text.Replace(" <color=white><</color>", "");

            // Set cursor to standard
            var cursorPointer = Addressables.LoadAssetAsync<Texture2D>("Cursor/Standard").WaitForCompletion();
            Cursor.SetCursor(cursorPointer, Vector2.zero, CursorMode.Auto);
        }
    }

    private async void RunButtonEffect(Object button)
    {
        switch (button.name)
        {
            case "Resume":
                TogglePause(false);
                break;
            case "Restart":
                if(await ConfirmBox("Are you sure you want to restart this game?\nYour custom settings will be carried over, but all progress will be lost."))
                {
                    DontDestroyOnLoad(GameManager.instance);
                    SceneManager.LoadScene("Scenes/Battle");
                }

                break;
            case "Itch":
                Application.OpenURL("https://jowsey.itch.io/endovelicus");
                break;
            case "Quit":
                if(await ConfirmBox("Are you sure you want to quit to the menu?\nYou will lose all game progress."))
                {
                    SceneManager.LoadScene("Scenes/MainMenu");
                }

                break;
        }
    }

    // i am once again asking that you do not look upon this file for advanced async programming techniques.
    public static async Task<bool> ConfirmBox(string body)
    {
        var confirmBox = Addressables.InstantiateAsync("Prefabs/UI/ConfirmBox", UIManager.instance.canvas.transform)
            .WaitForCompletion()
            .GetComponent<ConfirmBox>();

        confirmBox.body = body;

        bool? response = null;
        confirmBox.yesButton.onClick.AddListener(() => response = true);
        confirmBox.noButton.onClick.AddListener(() => response = false);

        while (response == null)
        {
            await Task.Delay(100);
        }

        return (bool) response;
    }

    private void TogglePause(bool toggle)
    {
        UnselectAllButtons();
        isPaused = toggle;
        canvasGroup.blocksRaycasts = toggle;

        if(toggle)
            menu.SetActive(true);

        canvasGroup.TweenCanvasGroupAlpha(toggle ? 1f : 0f, 0.25f).SetEaseCubicInOut().SetUseUnscaledTime(true);
        menu.TweenAnchoredPositionX(toggle ? 0 : -320, 0.25f).SetEaseCubicInOut().SetUseUnscaledTime(true).SetOnComplete(() =>
        {
            if(!toggle)
                menu.SetActive(false);
        });
        
        TimeManager.instance.ToggleTimeControls(!toggle);

        if(toggle)
            TimeManager.instance.pauseButton.onClick.Invoke();
        else
        {
            TimeManager.instance.playButton.onClick.Invoke();
            Cursor.SetCursor(Addressables.LoadAssetAsync<Texture2D>("Cursor/Standard").WaitForCompletion(), Vector2.zero, CursorMode.Auto);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Get every button in the menu, then for each one, register events for selection
        menuOptions = menu.GetComponentsInChildren<Button>().Where(b => b.interactable).ToList();
        foreach (var button in menuOptions)
        {
            var trigger = button.AddComponent<EventTrigger>();

            var enter = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            enter.callback.AddListener(data =>
            {
                SelectButton(button.gameObject);

                SoundManager.Play(SoundManager.GetAudioClip("SFX/ButtonHover"), 0.25f);

                // Set cursor to pointer
                var cursorPointer = Addressables.LoadAssetAsync<Texture2D>("Cursor/Pointer").WaitForCompletion();
                Cursor.SetCursor(cursorPointer, Vector2.zero, CursorMode.Auto);
            });

            var exit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            exit.callback.AddListener(data => UnselectAllButtons());

            var click = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            click.callback.AddListener(data => RunButtonEffect(button));

            trigger.triggers.Add(enter);
            trigger.triggers.Add(exit);
            trigger.triggers.Add(click);
        }

        TogglePause(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(!menu.activeSelf);
            return;
        }

        // Use arrow keys to navigate through menu
        
        // editor's note: what on earth
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            var selectedIndex = menuOptions.IndexOf(menuOptions.FirstOrDefault(b => b.GetComponentInChildren<TextMeshProUGUI>().color != Color.white));

            if(selectedIndex >= menuOptions.Count - 1) return;
            SelectButton(menuOptions[selectedIndex + 1].gameObject);
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            var selectedIndex = menuOptions.IndexOf(menuOptions.FirstOrDefault(b => b.GetComponentInChildren<TextMeshProUGUI>().color != Color.white));
            if(selectedIndex == -1) selectedIndex = menuOptions.Count;

            if(selectedIndex <= 0) return;
            SelectButton(menuOptions[selectedIndex - 1].gameObject);
        }

        if(Input.GetKeyUp(KeyCode.Return))
        {
            var selectedButton = menuOptions.FirstOrDefault(b => b.GetComponentInChildren<TextMeshProUGUI>().color != Color.white);

            if(selectedButton != null)
            {
                RunButtonEffect(selectedButton);
            }
        }
    }
}
