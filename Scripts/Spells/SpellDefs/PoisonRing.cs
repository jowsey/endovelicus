using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;

[UsedImplicitly]
public class PoisonRing : Spell
{
    public PoisonRing(int level = 1) : base(Icons.ResolveIcon(Icons.Bubbles), level, 5)
    {
        NewLevelValue("durationSeconds", new List<float> { 4, 5, 7, 9, 12 });
        NewLevelValue("damagePerSecond", new List<float> { 1.5f, 2f, 2.5f, 3f, 4f });
        NewLevelValue("scale", new List<float> { 1, 1.1f, 1.2f, 1.3f, 1.5f });

        NewLevelValue("cooldown", new List<float> { 12, 18, 22, 25, 30 });
        NewLevelValue("devotionCost", new List<float> { 12, 16, 18, 26, 40 });
    }

    public override string GetDescription() =>
        $"Harness the power of the underworld to damage units in a {4 * LevelValue("scale"):0.0}m circle for {FormatLevelValue("durationSeconds")}s.\nUnits in the circle take {FormatLevelValue("damagePerSecond")} DPS while inside.";

    public override int GetDevotionCost() => (int)LevelValue("devotionCost");

    public override int GetGoldCost() => 0;

    public override float GetCooldown() => (int)LevelValue("cooldown");

    public override IEnumerator CastEffect()
    {
        yield return GetMouseClickPosition(clickPosition =>
        {
            var ring = Addressables.InstantiateAsync("Prefabs/Spells/PoisonRing")
                .WaitForCompletion()
                .GetComponent<PoisonRingObject>();

            ring.transform.position = clickPosition;

            ring.duration = LevelValue("durationSeconds");
            ring.damagePerSecond = LevelValue("damagePerSecond");
            ring.transform.localScale *= LevelValue("scale");
        });

        yield return null;
    }
}
