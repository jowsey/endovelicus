using System;
using System.Collections.Generic;
using System.Linq;

public enum StatType
{
    UnitsSummoned,
    TowersBuilt,
    RomanUnitsKilled,
    FriendlyUnitsLost,
    VillagesLost,
    VillagesCaptured,
    SpellsUsed,
    UpgradesBought
}

public static class StatsManager
{
    public static readonly IDictionary<StatType, int> stats = new Dictionary<StatType, int>
    {
        {StatType.UnitsSummoned, 0},
        {StatType.TowersBuilt, 0},
        {StatType.RomanUnitsKilled, 0},
        {StatType.FriendlyUnitsLost, 0},
        {StatType.VillagesLost, 0},
        {StatType.VillagesCaptured, 0},
        {StatType.SpellsUsed, 0},
        {StatType.UpgradesBought, 0}
    };
    
    public static void Reset()
    {
        foreach (var stat in stats.ToArray())
        {
            stats[stat.Key] = 0;
        }
    }
}