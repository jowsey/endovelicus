using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public enum BuildingType
{
    House,
    Church
}

public enum Ownership
{
    Neutral,
    Friendly,
    Roman
}

public class Building : MonoBehaviour
{
    public BuildingType type;
    public Vector3 position;
}

public class Village : MonoBehaviour
{
    public Area area;
    public List<Building> buildings = new List<Building>();
    public LineRenderer borderRenderer;
    public CircleCollider2D borderTrigger;
    public Vector3 centerPosition;
    public List<Unit> unitsInArea = new List<Unit>();
    public Ownership ownership { get; private set; }

    // % that capture progress decreases by per second when not being captured and not at 100% progress
    public float progressReductionPerSecond = 1.75f;

    private float _takeoverProgress;
    public VillageDetailsDisplay detailsDisplay;

    public float takeoverProgress
    {
        get => _takeoverProgress;
        set
        {
            value = Mathf.Clamp(value, -100, 100);

            var originalValue = _takeoverProgress;
            _takeoverProgress = value;

            switch (value)
            {
                case 100 when ownership != Ownership.Friendly:
                    ownership = Ownership.Friendly;
                    onFriendlyCapture.Invoke();
                    break;
                case -100 when ownership != Ownership.Roman:
                    ownership = Ownership.Roman;
                    onEnemyCapture.Invoke();
                    break;
                default:
                    {
                        if ((value <= 0 && originalValue > 0) || (value >= 0 && originalValue < 0) && ownership != Ownership.Neutral)
                        {
                            ownership = Ownership.Neutral;
                            onMadeNeutral.Invoke();
                        }

                        break;
                    }
            }

            onTakeoverProgressChanged.Invoke();
        }
    }

    [HideInInspector]
    public UnityEvent onFriendlyCapture = new UnityEvent();

    [HideInInspector]
    public UnityEvent onEnemyCapture = new UnityEvent();

    [HideInInspector]
    public UnityEvent onMadeNeutral = new UnityEvent();

    [HideInInspector]
    public UnityEvent onTakeoverProgressChanged = new UnityEvent();

    [HideInInspector]
    public UnityEvent onOwnershipChanged = new UnityEvent();

    private void Awake()
    {
        onFriendlyCapture.AddListener(onOwnershipChanged.Invoke);
        onMadeNeutral.AddListener(onOwnershipChanged.Invoke);
        onEnemyCapture.AddListener(onOwnershipChanged.Invoke);
    }

    // Start is called before the first frame update
    private void Start()
    {
        borderRenderer = gameObject.AddComponent<LineRenderer>();
        borderRenderer.material = new Material(Shader.Find("Sprites/Default"));

        borderRenderer.startWidth = 0.05f;
        borderRenderer.endWidth = 0.05f;

        borderRenderer.positionCount = 90;
        borderRenderer.loop = true;

        // Make sure this always renders on top
        borderRenderer.sortingOrder = 999;

        // Get the radius of the village by finding the distance between the center and the furthest building, add a lil to make sure we cover everything
        var radius = buildings.Select(b => Vector3.Distance(b.position, centerPosition)).Max() + 2f;

        // Set the positions of the border renderer
        for (var i = 0; i < borderRenderer.positionCount; i++)
        {
            var angle = i * Mathf.PI * 2 / borderRenderer.positionCount;
            borderRenderer.SetPosition(i,
                new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0) + centerPosition);
        }

        // Set the border collider trigger to be the same size as the border
        borderTrigger = gameObject.AddComponent<CircleCollider2D>();
        borderTrigger.offset = centerPosition - transform.position;
        borderTrigger.radius = radius;
        borderTrigger.isTrigger = true;

        UpdateBorder();

        MapManager.instance.onFinishedLoading.AddListener(() =>
        {
            onOwnershipChanged.AddListener(() =>
            {
                UpdateBorder();
                StatsManager.stats[ownership == Ownership.Friendly ? StatType.VillagesCaptured : StatType.VillagesLost]++;
            });
            
            StartCoroutine(PaymentTimer());
        });
    }

    private void UpdateBorder()
    {
        var reducedOpacityColour = ownership switch
        {
            Ownership.Friendly => Constants.friendlyColour,
            Ownership.Roman => Constants.enemyColour,
            Ownership.Neutral => Constants.neutralColour,
            _ => throw new ArgumentOutOfRangeException()
        };

        reducedOpacityColour.a = 0.333f;
        borderRenderer.startColor = reducedOpacityColour;
        borderRenderer.endColor = reducedOpacityColour;
    }
    private void FixedUpdate()
    {
        // If the village is not being contested, reduce the takeover progression
        if (unitsInArea.Count == 0)
        {
            switch (ownership)
            {
                case Ownership.Friendly when takeoverProgress < 100:
                    takeoverProgress += Mathf.Min(progressReductionPerSecond * Time.deltaTime, 100 - takeoverProgress);
                    break;
                case Ownership.Roman when takeoverProgress > -100:
                    takeoverProgress -= Mathf.Min(progressReductionPerSecond * Time.deltaTime, -100 + takeoverProgress);
                    break;
                case Ownership.Neutral when takeoverProgress < 0:
                    takeoverProgress += Mathf.Min(progressReductionPerSecond * Time.deltaTime, -takeoverProgress);
                    break;
                case Ownership.Neutral when takeoverProgress > 0:
                    takeoverProgress -= Mathf.Min(progressReductionPerSecond * Time.deltaTime, takeoverProgress);
                    break;
            }
        }
        else
        {
            // For every unit *more* a team has in an area, they gain a point.
            var takeoverPower = (float)unitsInArea.Count(u => u.ownership == UnitOwnership.Friendly) -
                                unitsInArea.Count(u => u.ownership == UnitOwnership.Roman);

            // Each unit becomes less valuable as more units are used, to reduce efficiency of death balls
            // At 1 unit, each unit is worth 1p/s - by >=12 units, each new unit is worth 0.5p/s
            takeoverPower *= Mathf.Lerp(1f, 0.5f, takeoverPower / 12f);

            takeoverProgress += (takeoverPower * Time.deltaTime) * GameManager.instance.captureSpeedMultiplier;
        }

        unitsInArea.Clear();
    }

    private IEnumerator PaymentTimer()
    {
        while (true)
        {
            // Every 5th second
            yield return new WaitForSeconds(5f);

            if (ownership == Ownership.Friendly)
            {
                UIManager.instance.inventory.GetStatistic("gold").Count += buildings.Count(b => b.type == BuildingType.House);
                UIManager.instance.inventory.GetStatistic("devotion").Count += buildings.Count(b => b.type == BuildingType.Church);
            }
        }
    }

    // using stay because unity has a dumb bug around destroying objects and recognising OnTriggerExit
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out Unit unit))
        {
            unitsInArea.Add(unit);
        }
    }

    public void PlaceSummonOrder(SummonOrder summonOrder)
    {
        summonOrder.summoningVillage = this;

        var orderDisplay = Addressables.InstantiateAsync("Prefabs/UI/VillageOrder", detailsDisplay.ordersContainer.transform)
            .WaitForCompletion()
            .GetComponent<VillageOrder>();

        orderDisplay.order = summonOrder;
    }
}
