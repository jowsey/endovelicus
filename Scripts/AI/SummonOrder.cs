using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SummonOrder
{
    public Village summoningVillage;
    public Vector3 position;
    public readonly IDictionary<UnitClass, int> unitSets;
    public readonly UnitOwnership ownership;
    public readonly AIUnitGroup group;
    public readonly UnityEvent onComplete = new UnityEvent();

    public SummonOrder(Vector3 position, IDictionary<UnitClass, int> unitSets, UnitOwnership ownership, AIUnitGroup group = null)
    {
        this.position = position;
        this.unitSets = unitSets;
        this.ownership = ownership;
        this.group = group;
    }
}
