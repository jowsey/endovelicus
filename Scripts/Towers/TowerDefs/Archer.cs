using System.Collections.Generic;
using JetBrains.Annotations;

[UsedImplicitly]
public class Archer : TowerData
{
    public Archer() : base(UnitClass.Archer, Icons.ResolveIcon(Icons.HighShot), 5)
    {
        NewLevelValue("maxHealth", new List<float> {500, 550, 600, 650, 750});
        NewLevelValue("attackRange", new List<float> {12, 12.25f, 12.75f, 13.75f, 15f});
        NewLevelValue("attackDamage", new List<float> {6, 6.25f, 6.5f, 6.75f, 7f});
        NewLevelValue("attackCooldown", new List<float> {1.5f, 1.45f, 1.4f, 1.35f, 1.25f});

        NewLevelValue("goldCost", new List<float> {350, 150, 200, 250, 350});
    }

    public override string GetDescription() => "A simple watchtower, allowing an archer to watch over a wide area from a safe point.";

    public override int GetDevotionCost() => 3;

    public override int GetGoldCost() => (int) LevelValue("goldCost", 1);
}
