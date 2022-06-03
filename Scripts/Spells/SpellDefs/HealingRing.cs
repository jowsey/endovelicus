using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;

[UsedImplicitly]
public class HealingRing : Spell
{
    public HealingRing(int level = 1) : base(Icons.ResolveIcon(Icons.Healing), level, 5)
    {
        NewLevelValue("durationSeconds", new List<float> {3, 4, 6, 8, 10});
        NewLevelValue("healingPerSecond", new List<float>{2, 2, 3, 3, 4});
        NewLevelValue("scale", new List<float>{1, 1.1f, 1.2f, 1.3f, 1.5f});

        NewLevelValue("cooldown", new List<float> {9, 12, 18, 22, 24});
        NewLevelValue("devotionCost", new List<float> {12, 16, 22, 28, 36});
    }

    public override string GetDescription() =>
        $"Harness your rejuvinating power to heal units in a {4 * LevelValue("scale"):0.0}m circle for {FormatLevelValue("durationSeconds")}s.\nUnits in the circle gain {FormatLevelValue("healingPerSecond")}hp/s while inside.";

    public override int GetDevotionCost() => (int) LevelValue("devotionCost");

    public override int GetGoldCost() => 0;

    public override float GetCooldown() => (int) LevelValue("cooldown");

    public override IEnumerator CastEffect()
    {
        yield return GetMouseClickPosition(clickPosition =>
        {
            var ring = Addressables.InstantiateAsync("Prefabs/Spells/HealingRing")
                .WaitForCompletion()
                .GetComponent<HealingRingObject>();

            ring.transform.position = clickPosition;
            
            ring.duration = LevelValue("durationSeconds");
            ring.healingPerSecond = LevelValue("healingPerSecond");
            ring.transform.localScale *= LevelValue("scale");
        });

        yield return null;
    }
}
