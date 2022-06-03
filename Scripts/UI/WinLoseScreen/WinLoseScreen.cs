using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using ElRaccoone.Tweens;

public class WinLoseScreen : MonoBehaviour
{
    public RectTransform rt;
    public TextMeshProUGUI title;
    public TextMeshProUGUI statsText;
    public Button restartButton;
    public Button menuButton;
    public bool didWin;

    // Start is called before the first frame update
    private void Start()
    {
        gameObject.TweenLocalScaleX(1f, 0.5f)
            .SetEaseExpoIn()
            .SetUseUnscaledTime(true)
            .SetFrom(0f);

        TimeManager.instance.pauseButton.onClick.Invoke();
        TimeManager.instance.ToggleTimeControls(false);

        title.color = didWin ? Constants.winTextColor : Constants.loseTextColor;
        title.text = didWin
            ? "Lusitania lives another day.\n<size=50%>You successfully drove back the Roman invasion."
            : "The Roman Empire grows.\n<size=50%>You were unable to stop the Roman invasion.";
        
        var i = 0;
        foreach (var stat in StatsManager.stats)
        {
            statsText.text += $"\n<color=#{(i++ % 2 == 0 ? "DDD" : "FFF")}> {string.Join(" ", Regex.Split(stat.Key.ToString(), @"(?<!^)(?=[A-Z](?![A-Z]|$))"))}: {stat.Value}";
        }

        restartButton.onClick.AddListener(async () =>
        {
            if (await PauseMenu.ConfirmBox("Are you sure you want to restart the game?\nYour custom settings will be carried over, but all progress will be lost."))
            {
                DontDestroyOnLoad(GameManager.instance);
                SceneManager.LoadSceneAsync("Scenes/Battle", LoadSceneMode.Single);
            }
        });

        menuButton.onClick.AddListener(async () =>
        {
            if (await PauseMenu.ConfirmBox("Are you sure you want to return to the main menu?"))
            {
                SceneManager.LoadSceneAsync("Scenes/MainMenu", LoadSceneMode.Single);
            }
        });
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }
}
