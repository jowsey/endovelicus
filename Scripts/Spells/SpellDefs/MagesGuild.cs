using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

[UsedImplicitly]
public class MagesGuild : Spell
{
    public MagesGuild(int level = 1) : base(Icons.ResolveIcon(Icons.WizardStaff), level, 3)
    {
        NewLevelValue("mages", new List<float> { 1, 2, 4 });

        NewLevelValue("devotionCost", new List<float> { 30, 50, 100 });
        NewLevelValue("cooldown", new List<float> { 15, 18, 20 });
    }

    public override string GetDescription() =>
        $"Call upon the Mage's Guild for assistance.\nSummons {FormatLevelValue("mages")} mage(s).";

    public override int GetDevotionCost() => (int)LevelValue("devotionCost");

    public override int GetGoldCost() => 0;

    public override float GetCooldown() => LevelValue("cooldown");

    public override IEnumerator CastEffect()
    {
        yield return GetMouseClickPosition(clickPosition =>
        {
            var nearestVillage = MapManager.instance.villages
                .Where(v => v.ownership == Ownership.Friendly)
                .OrderBy(v => Vector3.Distance(v.centerPosition, clickPosition))
                .First();

            nearestVillage.PlaceSummonOrder(new SummonOrder(
                clickPosition,
                new Dictionary<UnitClass, int>
                {
                    {UnitClass.Mage, (int) LevelValue("mages")},
                },
                UnitOwnership.Friendly
            ));
        });

        yield return null;
    }

    public static IEnumerator SummonUnits(SummonOrder summonOrder)
    {
        foreach (var unitSet in summonOrder.unitSets)
        {
            for (var i = 0; i < unitSet.Value; i++)
            {
                var idealUnitPosition = summonOrder.summoningVillage.centerPosition + new Vector3(
                    Random.Range(-3f, 3f),
                    Random.Range(-3f, 3f),
                    0
                );

                NavMesh.SamplePosition(idealUnitPosition, out var hit, 10f, NavMesh.AllAreas);

                var unit = Addressables.InstantiateAsync("Prefabs/Units/Basic")
                    .WaitForCompletion()
                    .GetComponent<Unit>();

                unit.transform.position = hit.position;
                unit.navMeshAgent.Warp(hit.position);

                unit.ownership = summonOrder.ownership;
                unit.unitClass = unitSet.Key;

                if (summonOrder.group != null)
                {
                    summonOrder.group.AddUnit(unit);
                }
                else
                {
                    unit.navMeshAgent.stoppingDistance = 0.75f + i * 0.25f;
                    unit.mainDestination = summonOrder.position;
                }

                if (summonOrder.ownership == UnitOwnership.Friendly)
                {
                    unit.selected = true;
                    DragSelect.selectedUnits.Add(unit);
                }

                // Spawn units over the course of a second or so, looks nicer
                yield return new WaitForSeconds(0.2f);
            }
        }

        summonOrder.onComplete.Invoke();
        yield return null;
    }
}
