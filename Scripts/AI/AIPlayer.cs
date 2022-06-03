using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public static AIPlayer instance { get; private set; }

    public readonly List<AIUnitGroup> unitGroups = new List<AIUnitGroup>();
    public static int maxUnitGroups => GameManager.instance.villagesPerArea + Mathf.FloorToInt(LevelManager.instance.deityLevel / 3f);

    private readonly IDictionary<UnitClass, int> unitSetLevel1 = new Dictionary<UnitClass, int>
    {
        {UnitClass.Melee, 2},
        {UnitClass.Archer, 3}
    };

    private readonly IDictionary<UnitClass, int> unitSetLevel2 = new Dictionary<UnitClass, int>
    {
        {UnitClass.Melee, 3},
        {UnitClass.Archer, 4}
    };

    private readonly IDictionary<UnitClass, int> unitSetLevel3 = new Dictionary<UnitClass, int>
    {
        {UnitClass.Melee, 3},
        {UnitClass.Archer, 3},
        {UnitClass.Mage, 1}
    };

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start() => MapManager.instance.onFinishedLoading.AddListener(() => StartCoroutine(AILoop()));

    private IEnumerator AILoop()
    {
        // AI begins once player captures their first area thus presumably understands the basic game rules
        yield return new WaitUntil(() => MapManager.instance.areas[1].ownership == Ownership.Friendly);
        Debug.Log("<i><b><color=#DD2222>The Roman Empire</color></b> joins the battle!</i>");

        while (true)
        {
            yield return new WaitForSeconds(10f);
            if(unitGroups.Count < maxUnitGroups)
            {
                var romanVillages = MapManager.instance.villages.Where(v => v.ownership == Ownership.Roman).ToList();
                var furthestOwnedVillage = romanVillages
                    .OrderBy(v => v.centerPosition.x)
                    .Take(maxUnitGroups - unitGroups.Count);

                foreach (var village in furthestOwnedVillage)
                {
                    // if romans currently have no units, let them have a better group
                    if(unitGroups.Count == 0)
                    {
                        // if they have no units AND ~1 area, give them a *really* good group
                        if(romanVillages.Count <= GameManager.instance.villagesPerArea)
                        {
                            unitGroups.Add(new AIUnitGroup(village, unitSetLevel3));
                        }
                        else
                        {
                            unitGroups.Add(new AIUnitGroup(village, unitSetLevel2));
                        }
                    }
                    else
                    {
                        unitGroups.Add(new AIUnitGroup(village, unitSetLevel1));
                    }
                }
            }
        }
    }
}
