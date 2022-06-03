using System;
using UnityEngine;
using Discord;
using UnityEngine.SceneManagement;

public class DiscordPresence : MonoBehaviour
{
    public string applicationId;
    public static DiscordPresence instance { get; private set; }
    private Discord.Discord discord;
    private Activity activity;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        discord = new Discord.Discord(long.Parse(applicationId), (ulong) CreateFlags.NoRequireDiscord);
        
        activity = new Activity
        {
            Assets = new ActivityAssets
            {
                LargeImage = "icon",
                LargeText = "Endovelicus by Thomas Jowsey"
            },
            Timestamps =
            {
                Start = DateTimeOffset.Now.ToUnixTimeSeconds()
            }
        };

        SceneManager.activeSceneChanged += SceneChanged;
        SceneChanged(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
    }

    // Update is called once per frame
    private void Update()
    {
        discord?.RunCallbacks();
    }

    private void SceneChanged(Scene current, Scene next)
    {
        activity.State = next.name switch
        {
            "MainMenu" => "In the menus",
            "Battle" => "In a match",
            _ => ""
        };

        activity.Details = next.name switch
        {
            "Battle" => "Casting spells",
            _ => ""
        };

        activity.Timestamps.Start = DateTimeOffset.Now.ToUnixTimeSeconds();

        discord?.GetActivityManager().UpdateActivity(activity, result => { });
    }

    private void OnDestroy()
    {
        discord?.Dispose();
    }
}