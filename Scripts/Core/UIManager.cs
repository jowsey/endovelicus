using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }
    public Canvas canvas;
    public Camera cam;
    public StatisticCounterList inventory;
    public AreaTakeoverBar areaTakeoverBar;
    public Canvas villageDetailsCanvas;
    public SpellUI spellUI;
    public TowerMenu towerMenu;

    private void Awake()
    {
        // We make sure there can only ever be one instance of this class by destroying it when created if one already exists
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        inventory.RegisterStatistic("gold", Icons.ResolveIcon(Icons.TwoCoins), 75);
        inventory.RegisterStatistic("devotion", Icons.ResolveIcon(Icons.Church), 30);

        spellUI.AddSpell<Bribe>(1);
        spellUI.AddSpell<Summon>(1);

        towerMenu.AddTowerType<Archer>();
    }
}
