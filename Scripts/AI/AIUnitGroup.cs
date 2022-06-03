using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AIUnitGroup
{
    private readonly List<Unit> units = new List<Unit>();
    private Unit leader => units.First();
    private Village currentlyTargettedVillage;

    private Village FindClosestUncapturedVillage()
    {
        // 25/75 chance to prioritise closest village or village furthest to right
        // 1/4 of groups will focus on expanding, 3/4 will be on "defence duty"
        // making sure they never over-extend which would feel unfair since
        // the player can't because of the fog (which AI does not care about)
        if (Random.value >= 0.75f)
        {
            return MapManager.instance.villages
                .Where(v => v.ownership != Ownership.Roman)
                .OrderBy(v => Vector3.Distance(v.centerPosition, leader.transform.position))
                .First();
        }
        else
        {
            return MapManager.instance.villages
                .Where(v => v.ownership != Ownership.Roman)
                .OrderBy(v => v.centerPosition.x)
                .Last();
        }
    }

    public AIUnitGroup(Village village, IDictionary<UnitClass, int> units)
    {
        var summonOrder = new SummonOrder(village.centerPosition, units, UnitOwnership.Roman, this);
        summonOrder.onComplete.AddListener(AttackClosestVillage);

        village.PlaceSummonOrder(summonOrder);
    }

    private void AssignGroupPath(Vector3 point)
    {
        NavMesh.SamplePosition(point, out var pointHit, 10f, NavMesh.AllAreas);
        NavMesh.SamplePosition(leader.transform.position, out var leaderHit, 10f, NavMesh.AllAreas);

        var path = new NavMeshPath();
        NavMesh.CalculatePath(leaderHit.position, pointHit.position, NavMesh.AllAreas, path);

        // Debug.Log($"{path.corners.Length} corners");

        for (var i = 0; i < units.Count; i++)
        {
            var unit = units[i];

            unit.orderType = UnitOrder.AttackMove;
            unit.mainPath = path;
            unit.navMeshAgent.stoppingDistance = 1f + 0.25f * i;
        }
    }

    private void AttackClosestVillage()
    {
        if (currentlyTargettedVillage != null)
        {
            currentlyTargettedVillage.onEnemyCapture.RemoveListener(AttackClosestVillage);
            currentlyTargettedVillage = null;
        }

        var closestVillage = FindClosestUncapturedVillage();
        currentlyTargettedVillage = closestVillage;

        AssignGroupPath(closestVillage.centerPosition);

        // Attack another village once this one is captured
        closestVillage.onEnemyCapture.AddListener(AttackClosestVillage);
    }

    public void AddUnit(Unit unit)
    {
        unit.unitGroup = this;
        units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);

        if (units.Count == 0)
        {
            if (currentlyTargettedVillage != null)
            {
                currentlyTargettedVillage.onEnemyCapture.RemoveListener(AttackClosestVillage);
                currentlyTargettedVillage = null;
            }

            AIPlayer.instance.unitGroups.Remove(this);
        }
    }
}
