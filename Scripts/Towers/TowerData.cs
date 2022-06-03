using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class TowerData
{
    protected TowerData(UnitClass unitClass, Sprite icon = null, int maxLevel = 1)
    {
        _name = GetType().Name;

        UnitClass = unitClass;
        Icon = icon;
        MaxLevel = maxLevel;
    }

    // Stats by level, where list is value ordered by levels ascending
    private readonly List<KeyValuePair<string, List<float>>> LevelledStats = new List<KeyValuePair<string, List<float>>>();

    // Type of unit to use in tower
    public UnitClass UnitClass;

    // Name of tower
    private readonly string _name;
    public string Name => string.Join(" ", Regex.Split(_name, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));

    // Icon of tower to use in tower menu
    public readonly Sprite Icon;

    // Max level of tower
    public readonly int MaxLevel;

    // Description of what tower does
    public abstract string GetDescription();

    // How much devotion is required to build tower
    public abstract int GetDevotionCost();

    // How much gold is required to build tower
    public abstract int GetGoldCost();

    // Get value of stat for specific level
    public float LevelValue(string statName, int level)
    {
        // Get list of values for selected stat
        var values = LevelledStats.Find(x => x.Key == statName).Value;

        // Return value for selected level or highest level if requested is not available
        return values[Mathf.Min(level - 1, values.Count - 1)];
    }

    // Set value based on tower level
    protected void NewLevelValue(string statName, List<float> values) => LevelledStats.Add(new KeyValuePair<string, List<float>>(statName, values));

    protected string FormatLevelValue(string statName, int level)
    {
        return $"<color=#C7DCD0>{LevelValue(statName, level)}</color>";
    }

    public static IEnumerator GetMouseClickGridPosition(TowerData towerData, Action<Vector3Int> callback)
    {
        RelevantControlsList.instance.Add(ControlTips.ChooseTowerPosition);
        RelevantControlsList.instance.Add(ControlTips.CancelTowerPlacement);

        var originalTimeScale = Time.timeScale;

        // Tween time scale lower to give player time to think
        TimeManager.instance.SetSpeed(0.1f);

        Cursor.SetCursor(Addressables.LoadAssetAsync<Texture2D>("Cursor/Pointer").WaitForCompletion(), Vector2.zero, CursorMode.Auto);

        var placeholderTower = Addressables.InstantiateAsync("Prefabs/Towers/PlaceholderTower")
            .WaitForCompletion()
            .GetComponent<PlaceholderTower>();

        placeholderTower.towerData = towerData;

        while (true)
        {
            yield return null;
            if (Input.GetMouseButtonDown(1))
            {
                placeholderTower.position = Vector3Int.zero;
                placeholderTower.End();
                break;
            }

            var mousePosition = Vector3Int.FloorToInt(UIManager.instance.cam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, 10f)));
            if (placeholderTower.position != mousePosition) placeholderTower.position = mousePosition;

            if (!Input.GetMouseButtonDown(0) || !placeholderTower.CheckPositionsAreFree()) continue;

            break;
        }

        // Get mouse click position
        var clickPosition = placeholderTower.position;

        Cursor.SetCursor(Addressables.LoadAssetAsync<Texture2D>("Cursor/Standard").WaitForCompletion(), Vector2.zero, CursorMode.Auto);

        // Tween time scale back to normal
        TimeManager.instance.SetSpeed(originalTimeScale);

        RelevantControlsList.instance.Remove(ControlTips.ChooseTowerPosition);
        RelevantControlsList.instance.Remove(ControlTips.CancelTowerPlacement);

        placeholderTower.End();
        callback(clickPosition);
        yield return null;
    }
}
