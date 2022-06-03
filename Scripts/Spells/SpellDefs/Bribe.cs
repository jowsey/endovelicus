using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly]
public class Bribe : Spell
{
    public Bribe(int level = 1) : base(Icons.ResolveIcon(Icons.TwoCoins), level, 5)
    {
        NewLevelValue("goldCost", new List<float> {30, 60, 85, 110, 120});
        NewLevelValue("devotionGiven", new List<float> {10, 20, 30, 40, 50});
        
        NewLevelValue("cooldown", new List<float> {10, 15, 20, 25, 30});
    }

    public override string GetDescription() =>
        $"Bribe your followers with gold in return for devotion.\nYou gain {FormatLevelValue("devotionGiven")} devotion instantly.";

    public override int GetDevotionCost() => 0;

    public override int GetGoldCost() => (int) LevelValue("goldCost");

    public override float GetCooldown() => (int) LevelValue("cooldown");

    public override IEnumerator CastEffect()
    {
        UIManager.instance.inventory.GetStatistic("devotion").Count += (int) LevelValue("devotionGiven");

        yield return null;
    }
}
