using UnityEngine;

public enum TargetType
{
    Unit,
    Tower
}

// presumably a better way of doing this would be using an interface or abstract class or something else
// but i have very little time left so i'm doing whatever is quickest to bolt onto what i already have
public class Target : MonoBehaviour
{
    public UnitOwnership ownership;

    private Unit _unit;

    public Unit unit
    {
        get => _unit;
        set
        {
            _unit = value;
            targetType = TargetType.Unit;
            ownership = value.ownership;
        }
    }

    private Tower _tower;
    public Tower tower
    {
        get => _tower;
        set
        {
            _tower = value;
            targetType = TargetType.Tower;
            ownership = Tower.ownership;
        }
    }

    public TargetType targetType;
    
    public float health
    {
        get => targetType == TargetType.Unit ? unit.health : tower.health;
        set
        {
            if(targetType == TargetType.Unit)
            {
                unit.health = value;
            }
            else
            {
                tower.health = value;
            }
        }
    }
}
