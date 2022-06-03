using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RomanNumerals;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public abstract class Spell
{
    protected Spell(Sprite icon = null, int level = 1, int maxLevel = 1)
    {
        _name = GetType().Name;

        Icon = icon;
        MaxLevel = maxLevel;
        Level = level;
    }

    public readonly UnityEvent onLevelChange = new UnityEvent();
    public readonly UnityEvent onCooldownEnd = new UnityEvent();

    // Stats by level, where list is value ordered by levels ascending
    private readonly List<KeyValuePair<string, List<float>>> LevelledStats = new List<KeyValuePair<string, List<float>>>();

    // Name of spell
    private readonly string _name;
    public string Name => string.Join(" ", Regex.Split(_name, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));
    
    public string LevelledName => Level == 1 ? Name : $"{Name} {new RomanNumeral(Level)}";

    // Icon of spell to use in SpellsUI
    public Sprite Icon;

    // Level of spell
    private int _level;

    public int Level
    {
        get => _level;
        set
        {
            if (value > MaxLevel)
            {
                Debug.LogWarning($"Tried to set level over maxlevel ({value} > {MaxLevel}) for spell {Name}");
            }

            CooldownEndTime = 0;
            onCooldownEnd.Invoke();
            
            _level = Mathf.Clamp(value, 1, MaxLevel);
            onLevelChange.Invoke();
        }
    }

    // Max level of spell
    public readonly int MaxLevel;

    // Timestamp that spell becomes available to use again
    public float CooldownEndTime;

    // Description of what spell does
    public abstract string GetDescription();

    // How much devotion is required to cast spell
    public abstract int GetDevotionCost();

    // How much gold is required to cast spell
    public abstract int GetGoldCost();

    // Cooldown between spell casts in seconds
    public abstract float GetCooldown();

    // Get value of stat for specific level
    protected float LevelValue(string statName, int level = default)
    {
        if (level == default) level = Level;

        // Get list of values for selected stat
        var values = LevelledStats.Find(x => x.Key == statName).Value;

        // Return value for selected level or highest level if requested is not available
        return values[Mathf.Min(level - 1, values.Count - 1)];
    }

    // Set value based on spell level
    protected void NewLevelValue(string statName, List<float> values) => LevelledStats.Add(new KeyValuePair<string, List<float>>(statName, values));
    
    protected string FormatLevelValue(string statName, int level = default)
    {
        if (level == default) level = Level;

        return $"<color=#C7DCD0>{LevelValue(statName, level)}</color>";
    }

    // Spell cast effect
    public abstract IEnumerator CastEffect();

    protected static IEnumerator GetMouseClickPosition(Action<Vector3> callback)
    {
        RelevantControlsList.instance.Add(ControlTips.CastLocationalSpell);

        var originalTimeScale = Time.timeScale;

        // Tween time scale lower to give player time to think
        TimeManager.instance.SetSpeed(0.1f);

        Cursor.SetCursor(Addressables.LoadAssetAsync<Texture2D>("Cursor/Pointer").WaitForCompletion(), Vector2.zero, CursorMode.Auto);

        // Wait 1 frame since the mouse button is inherently going to be down on the first frame
        yield return new WaitForEndOfFrame();

        // Wait for mouse click
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // Get mouse click position
        var clickPosition = UIManager.instance.cam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, 10f));

        Cursor.SetCursor(Addressables.LoadAssetAsync<Texture2D>("Cursor/Standard").WaitForCompletion(), Vector2.zero, CursorMode.Auto);

        // Tween time scale back to normal
        TimeManager.instance.SetSpeed(originalTimeScale);

        RelevantControlsList.instance.Remove(ControlTips.CastLocationalSpell);
        callback(clickPosition);
        yield return null;
    }
}
