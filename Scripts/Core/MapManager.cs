using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ElRaccoone.Tweens;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

public class MapManager : MonoBehaviour
{
    public static MapManager instance { get; private set; }

    [Header("Village generation")]
    [SerializeField]
    private int minVillageDistanceFromBorder = 16;

    [SerializeField]
    private int minDistanceBetweenVillages = 24;

    [SerializeField]
    private int maxFindVillageTileAttempts = 10000;

    [SerializeField]
    private int maxFindBuildingAreaAttempts = 10000;

    [Header("Road generation")]
    [SerializeField]
    private int roadWidth = 2;

    [Header("Forest generation")]
    [SerializeField]
    [Range(0f, 1f)]
    private float forestDensity = 0.42f;

    private Tilemap groundTilemap;
    private Tilemap obstacleTilemap;

    // Stores a list of lists of villages, each sub-list representing an area
    public readonly List<List<Vector3Int>> villagePositionsByArea = new List<List<Vector3Int>>();
    public readonly List<Vector3Int> globalReservedAreas = new List<Vector3Int>();

    public List<Village> villages = new List<Village>();
    public List<Area> areas = new List<Area>();

    private bool NotifiedAboutVillageCount;
    private bool NotifiedAboutBuildingCount;

    public NavMeshSurface nms;

    public Canvas mapGenerationCanvas;
    public TextMeshProUGUI statusText;

    public UnityEvent onFinishedLoading = new UnityEvent();
    public bool loadingFinished;

    // Start is called before the first frame update
    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        nms.hideEditorLogs = true;
    }

    private IEnumerator Start()
    {
        var game = GameManager.instance;

        TimeManager.instance.pauseButton.onClick.Invoke();

        var mainCanvasGroup = UIManager.instance.canvas.GetComponent<CanvasGroup>();
        UIManager.instance.canvas.enabled = false;
        mainCanvasGroup.TweenCanvasGroupAlpha(0f, 0f);

        // Debugging: Count how long it takes to do stuff, to help me optimise
        var initTimer = Stopwatch.StartNew();

        statusText.text = "Initialising...";
        yield return null;

        // Set the random seed
        Random.InitState(game.seed.GetHashCode());
        Debug.Log($"Using seed: {game.seed.GetHashCode()}");

        // Load assets from addressables build
        groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        obstacleTilemap = GameObject.Find("Obstacles").GetComponent<Tilemap>();

        var grassTiles = new[]
        {
            Addressables.LoadAssetAsync<Tile>("Tiles/Grass").WaitForCompletion(),
            Addressables.LoadAssetAsync<Tile>("Tiles/GrassUnique1").WaitForCompletion(),
            Addressables.LoadAssetAsync<Tile>("Tiles/GrassUnique2").WaitForCompletion(),
            Addressables.LoadAssetAsync<Tile>("Tiles/GrassUnique3").WaitForCompletion(),
        };

        var graveTile = Addressables.LoadAssetAsync<Tile>("Tiles/GraveCross1").WaitForCompletion();

        var pathCenterTile = Addressables.LoadAssetAsync<RuleTile>("Tiles/PathCenter").WaitForCompletion();
        var treeTile = Addressables.LoadAssetAsync<RuleTile>("Tiles/Tree").WaitForCompletion();

        var churchPrefab = Addressables.LoadAssetAsync<GameObject>("Prefabs/Church").WaitForCompletion();
        var housePrefab = Addressables.LoadAssetAsync<GameObject>("Prefabs/House").WaitForCompletion();

        initTimer.Stop();
        Debug.Log($"<b>Initialisation</b> took <b>{initTimer.ElapsedMilliseconds}</b>ms");

        UIManager.instance.cam.transform.Translate(new Vector3((game.areaWidth * game.areaCount) / 2f, game.areaHeight / 2f, 0f));
        UIManager.instance.cam.orthographicSize = game.areaHeight / 2f;

        var areaTimer = Stopwatch.StartNew();

        // Loop through every area
        for (var a = 0; a < game.areaCount; a++)
        {
            statusText.text = $"Generating area {a + 1} of {game.areaCount}...";
            yield return null;

            var areaObject = new GameObject("Area " + (a + 1))
            {
                transform =
                {
                    parent = transform
                }
            };

            areaObject.transform.SetSiblingIndex(transform.childCount - 2);

            var areaScript = areaObject.AddComponent<Area>();
            areaScript.index = a;

            areas.Add(areaScript);

            // Step 1: Just fill the map with grass
            for (var w = 0; w < game.areaWidth; w++)
            {
                for (var h = game.areaHeight - 1; h >= 0; h--)
                {
                    // 85% chance for standard grass, 15% chance to choose between the unique grasses
                    groundTilemap.SetTile(new Vector3Int(w + game.areaWidth * a, h, 0),
                        Random.value <= 0.85f ? grassTiles[0] : grassTiles[Random.Range(1, grassTiles.Length)]);
                }
            }

            // Create new list of village centers for this area
            villagePositionsByArea.Add(new List<Vector3Int>());

            // Step 2, generate village centers (grave tiles) in each area
            var topLeft = new Vector2Int(game.areaWidth * a, game.areaHeight);
            var bottomRight = new Vector2Int(game.areaWidth * (a + 1), 0);

            for (var i = 0; i < game.villagesPerArea; i++)
            {
                // Get random tile in the area
                // Make sure the tile is grass, is far away from the edge of the map, and not too close to another village
                Vector3Int targetTilePos = default;

                var attempts = 0;

                var isGrassTile = false;
                var isTooCloseToOtherVillage = false;

                while (!isGrassTile || isTooCloseToOtherVillage)
                {
                    // If we can't find a suitable tile, just skip this village
                    if (attempts >= maxFindVillageTileAttempts)
                    {
                        if (!NotifiedAboutVillageCount)
                        {
                            NotificationManager.instance.SendNotification(
                                "Unable to find a suitable tile for one or more villages.\nConsider generating less villages for a map of this size!",
                                Icons.ResolveIcon(Icons.PositionMarker)
                            );
                            NotifiedAboutVillageCount = true;
                        }

                        break;
                    }

                    // Generate random position in area
                    targetTilePos = new Vector3Int(
                        Random.Range(topLeft.x + minVillageDistanceFromBorder,
                            bottomRight.x - minVillageDistanceFromBorder),
                        Random.Range(topLeft.y - minVillageDistanceFromBorder,
                            bottomRight.y + minVillageDistanceFromBorder),
                        2
                    );

                    isGrassTile = grassTiles.Contains(groundTilemap.GetTile(targetTilePos - new Vector3Int(0, 0, targetTilePos.z)));
                    isTooCloseToOtherVillage = villagePositionsByArea.Last()
                        .Any(v => Vector3.Distance(v, targetTilePos) < minDistanceBetweenVillages);

                    attempts++;
                }

                // If no target tile was set, skip to next village
                if (!isGrassTile || isTooCloseToOtherVillage) continue;

                groundTilemap.SetTile(targetTilePos, graveTile);
                villagePositionsByArea.Last().Add(targetTilePos - new Vector3Int(0, 0, targetTilePos.z));
            }

            yield return null;
        }

        areaTimer.Stop();
        Debug.Log($"<b>Creating all areas</b> took <b>{areaTimer.ElapsedMilliseconds}</b>ms");

        var roadGenTimer = Stopwatch.StartNew();

        // Connect each area to the next one via the middle-most village
        // For each list of village centers, only keep the point which is closest to GameManager.instance.areaHeight / 2 (the center of the map, height-wise)
        var middleVillageCenters = villagePositionsByArea
            .Select(centers => centers.OrderBy(v => Mathf.Abs(v.y - game.areaHeight / 2)).First())
            .ToList();

        // Step 3: Generate road tiles between each point
        for (var i = 0; i < middleVillageCenters.Count - 1; i++)
        {
            statusText.text = $"Generating road section {i + 1} of {middleVillageCenters.Count}...";
            yield return null;

            // Starting point (left village)
            var p1 = middleVillageCenters[i];
            // Ending point (right village)
            var p2 = middleVillageCenters[i + 1];

            // For each integer point between the two points, place a center road tile there
            var roadCenterTilePoints = GetTilesInLine(p1.x, p1.y, p2.x, p2.y).ToList();

            // How many pieces of road on either side of the center tile
            var roadSideWidth = (int)Mathf.Floor(roadWidth / 2f);
            var roadWidthIsEven = roadWidth % 2 == 0;

            Vector3Int prevTilePos = default;

            // For each vertical strip of tiles in the road
            foreach (var tilePos in roadCenterTilePoints)
            {
                // For each tile in this strip
                // If width is even, remove the top layer of tiles
                for (var y = -roadSideWidth; y <= roadSideWidth - (roadWidthIsEven ? 1 : 0); y++)
                {
                    // 2% chance to not place a tile, for that cracked broken down aesthetic
                    if (Random.value <= 0.02f) continue;

                    groundTilemap.SetTile(tilePos + new Vector3Int(0, y, 0), pathCenterTile);

                    // Remove special grass tiles from under the road
                    groundTilemap.SetTile(tilePos + new Vector3Int(0, y, -tilePos.z), grassTiles[0]);

                    // Reserve road area
                    globalReservedAreas.Add(tilePos + new Vector3Int(0, y, -tilePos.z));
                }

                // Smooth inclines/declines by padding corners with an extra tile
                // If last point was higher than the current point
                var isLastPointHigher = prevTilePos != default && prevTilePos.y > tilePos.y;
                var isLastPointLower = prevTilePos != default && prevTilePos.y < tilePos.y;

                if (isLastPointHigher)
                {
                    // Set road tiles
                    groundTilemap.SetTile(tilePos + new Vector3Int(0, roadSideWidth + (roadWidthIsEven ? 0 : 1), 0), pathCenterTile);
                    groundTilemap.SetTile(tilePos + new Vector3Int(-1, -roadSideWidth, 0), pathCenterTile);

                    // Remove special grass tiles from under the road
                    groundTilemap.SetTile(tilePos + new Vector3Int(0, roadSideWidth + (roadWidthIsEven ? 0 : 1), -tilePos.z), grassTiles[0]);
                    groundTilemap.SetTile(tilePos + new Vector3Int(-1, -roadSideWidth, -tilePos.z), grassTiles[0]);

                    // Reserve road area
                    globalReservedAreas.Add(tilePos + new Vector3Int(0, roadSideWidth + (roadWidthIsEven ? 0 : 1), -tilePos.z));
                    globalReservedAreas.Add(tilePos + new Vector3Int(-1, -roadSideWidth, -tilePos.z));
                }
                // If last point was lower than the current point
                else if (isLastPointLower)
                {
                    // Set road tiles
                    groundTilemap.SetTile(tilePos + new Vector3Int(-1, roadSideWidth - (roadWidthIsEven ? 1 : 0), 0), pathCenterTile);
                    groundTilemap.SetTile(tilePos + new Vector3Int(0, -roadSideWidth, 0), pathCenterTile);

                    // Remove special grass tiles from under the road
                    groundTilemap.SetTile(tilePos + new Vector3Int(-1, roadSideWidth - (roadWidthIsEven ? 1 : 0), -tilePos.z), grassTiles[0]);
                    groundTilemap.SetTile(tilePos + new Vector3Int(0, -roadSideWidth, -tilePos.z), grassTiles[0]);

                    // Reserve road area
                    globalReservedAreas.Add(tilePos + new Vector3Int(-1, roadSideWidth - (roadWidthIsEven ? 1 : 0), -tilePos.z));
                    globalReservedAreas.Add(tilePos + new Vector3Int(0, -roadSideWidth, -tilePos.z));
                }

                prevTilePos = tilePos;
            }

            // Generate towers to the left and right of the road coming from the player's village
            if (i != 0) continue;

            var dir = (Vector3)p2 - p1;
            var cross = Vector3.Cross(dir, Vector3.back).normalized;

            var pointOnPath = dir.normalized * (minDistanceBetweenVillages / 2f);

            var topTowerPos = p1 + pointOnPath + cross * 3;
            var botTowerPos = p1 + pointOnPath + cross * -4;

            for (var j = 0; j < 2; j++)
            {
                var tower = Addressables.InstantiateAsync("Prefabs/Towers/Tower")
                    .WaitForCompletion()
                    .GetComponent<Tower>();

                var pos = Vector3Int.FloorToInt(j == 0 ? topTowerPos : botTowerPos);

                tower.transform.position = pos + new Vector3Int(1, 1, 0);
                tower.towerData = TowerMenu.instance.Get<Archer>();
                
                while (tower.level < tower.towerData.MaxLevel)
                    tower.Upgrade();

                for (var x = -1; x <= 2; x++)
                {
                    for (var y = -1; y <= 2; y++)
                    {
                        tower.reservedAreas.Add(pos + new Vector3Int(x, y, 0));
                    }
                }

                globalReservedAreas.AddRange(tower.reservedAreas);
            }
        }

        roadGenTimer.Stop();
        Debug.Log($"<b>Generating main roads</b> took <b>{roadGenTimer.ElapsedMilliseconds}</b>ms");

        var villageGenTimer = Stopwatch.StartNew();

        var allVillageReservedAreas = new List<Vector3Int>();

        // Step 4: Generate village content
        for (var ai = 0; ai < villagePositionsByArea.Count; ai++)
        {
            var villagesInArea = villagePositionsByArea[ai];
            var areaScript = transform.GetChild(ai).GetComponent<Area>();

            for (var vi = 0; vi < villagesInArea.Count; vi++)
            {
                var villageCenter = villagesInArea[vi];
                statusText.text = $"Generating buildings for village {vi + 1} of {villagesInArea.Count} in area {areaScript.index + 1}...";
                yield return null;

                // Generate GameObject to store all village stuff in
                var villageObject = new GameObject("Village " + vi)
                {
                    transform =
                    {
                        parent = transform.GetChild(ai),
                        position = new Vector3(villageCenter.x, villageCenter.y, 0)
                    }
                };

                var villageScript = villageObject.AddComponent<Village>();
                villageScript.area = areaScript;

                areaScript.villages.Add(villageScript);
                villages.Add(villageScript);

                // First area is friendly, last is roman, middles are all neutral
                villageScript.takeoverProgress = areaScript.index == 0 ? 100 : areaScript.index == game.areaCount - 1 ? -100 : 0;

                // Generate lil plaza in a 3x3 around the center 
                for (var x = -1; x <= 1; x++)
                {
                    for (var y = -1; y <= 1; y++)
                    {
                        groundTilemap.SetTile(villageCenter + new Vector3Int(x, y, -villageCenter.z + 1), pathCenterTile);

                        // Replace any fancy grass tiles with the plain flat one to prevent weird layering
                        groundTilemap.SetTile(villageCenter + new Vector3Int(x, y, -villageCenter.z), grassTiles[0]);
                    }
                }

                // Generate a list of areas around the town to reserve for buildings, in an <minDistanceBetweenVillages radius
                // Make sure each area doesn't overlap with other areas
                // The list stores the top left corner of each area
                var lotPoints = new List<Vector3Int>();

                const int buildingWidth = 3;
                const int buildingHeight = 3;
                var maxRadius = minDistanceBetweenVillages / 2 - buildingWidth - buildingHeight;

                // Keeps track of every tile that has been claimed to ensure we can't build on one that is already taken
                var villageReservedAreas = new List<Vector3Int>();

                // Add plaza to reserved areas with a 1 tile buffer
                for (var x = -1; x <= 1; x++)
                {
                    for (var y = -1; y <= 1; y++)
                    {
                        villageReservedAreas.Add(villageCenter + new Vector3Int(x, y, -villageCenter.z));
                    }
                }

                // For each lot in the village
                for (var li = 0; li < game.lotsPerVillage; li++)
                {
                    var attempts = 0;
                    var isOverlapping = true;

                    Vector3Int buildingPoint = default;
                    var mainReservedAreas = new List<Vector3Int>();

                    while (isOverlapping)
                    {
                        // Reset reserved area
                        mainReservedAreas.Clear();

                        if (attempts > maxFindBuildingAreaAttempts)
                        {
                            if (!NotifiedAboutBuildingCount)
                            {
                                NotificationManager.instance.SendNotification(
                                    "Unable to find a spot for one or more buildings.\nConsider generating less buildings for more consistent building counts!",
                                    Icons.ResolveIcon(Icons.PositionMarker)
                                );
                                NotifiedAboutBuildingCount = true;
                            }

                            break;
                        }

                        // Find random area in radius of center
                        buildingPoint = new Vector3Int(
                            villageCenter.x + Random.Range(-maxRadius, maxRadius),
                            villageCenter.y + Random.Range(-maxRadius, maxRadius),
                            1
                        );

                        // Reserve all tiles in the area
                        // Add 1 to each side to leave a 1 tile buffer between buildings for the path
                        var bufferedReservedAreas = new List<Vector3Int>();
                        for (var x = -1; x <= buildingWidth; x++)
                        {

                            for (var y = -1; y <= buildingHeight; y++)
                            {
                                // buffered list includes the extra tiles around - we use these to check for overlap, but don't actually reserve them
                                // this means we can check for 1 tile of overlap, instead of adding 2 buildings' overlaps for a 2-tile buffer
                                // we *do* add the bottom y-buffer though, for the building's path
                                bufferedReservedAreas.Add(buildingPoint + new Vector3Int(x, y, -buildingPoint.z));

                                if (y < buildingHeight && x > -1 && x < buildingWidth)
                                {
                                    mainReservedAreas.Add(buildingPoint + new Vector3Int(x, y, -buildingPoint.z));
                                }
                            }
                        }

                        // Make sure the area doesn't overlap with anything
                        isOverlapping = bufferedReservedAreas.Any(point =>
                            villageReservedAreas.Contains(point) || globalReservedAreas.Contains(point));

                        attempts++;
                    }

                    // Don't add building if we couldn't find a spot for it
                    if (mainReservedAreas.Count == 0) continue;

                    villageReservedAreas.AddRange(mainReservedAreas);
                    lotPoints.Add(buildingPoint + new Vector3Int(0, 0, -buildingPoint.z));
                }

                allVillageReservedAreas.AddRange(villageReservedAreas);

                var closestLotPointToPlaza = lotPoints.OrderBy(point => Vector3Int.Distance(point, villageCenter)).First();

                foreach (var lotPoint in lotPoints)
                {
                    // Place roads below building to make a path at their front door
                    for (var y = -1; y <= 0; y++)
                    {
                        for (var x = -1; x <= buildingWidth; x++)
                        {
                            // Looks nicer without top corners
                            if (y == 0 && (x == -1 || x == buildingWidth)) continue;

                            groundTilemap.SetTile(lotPoint + new Vector3Int(x, y, -lotPoint.z + 1), pathCenterTile);

                            // Replace any fancy grass tiles with the plain flat one to prevent weird layering
                            groundTilemap.SetTile(lotPoint + new Vector3Int(x, y, -lotPoint.z), grassTiles[0]);
                        }
                    }

                    // Add a building to the lot
                    var isChurch = lotPoint == closestLotPointToPlaza;

                    var building = Instantiate(isChurch ? churchPrefab : housePrefab, lotPoint, Quaternion.identity);
                    building.transform.SetParent(villageObject.transform);

                    var buildingScript = building.AddComponent<Building>();
                    buildingScript.type = isChurch ? BuildingType.Church : BuildingType.House;
                    buildingScript.position = lotPoint + new Vector3(buildingWidth / 2f, 0, 0);

                    // Register building to village
                    villageScript.buildings.Add(buildingScript);
                }

                // Get center point of village by averaging all building positions
                villageScript.centerPosition = villageScript.buildings
                    .Select(b => b.position)
                    .Aggregate((sum, current) => sum + current) / villageScript.buildings.Count;
            }
        }

        villageGenTimer.Stop();
        Debug.Log($"<b>Village generation</b> took <b>{villageGenTimer.ElapsedMilliseconds}</b>ms");

        var forestGenTimer = Stopwatch.StartNew();
        var forestReservedAreas = new List<Vector3Int>();

        var noiseGen = new FastNoiseLite(game.seed.GetHashCode());
        noiseGen.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        var mapWidth = game.areaWidth * game.areaCount;
        var mapHeight = game.areaHeight;

        const int scale = 8;

        for (var y = mapHeight - 1; y >= 0; y--)
        {
            for (var x = 0; x < mapWidth; x++)
            {
                var noiseVal = (noiseGen.GetNoise(x * scale, y * scale) + 1f) / 2;

                var isNearVillage = villagePositionsByArea.Any(a =>
                    a.Any(v => Vector3.Distance(new Vector3(x, y, 0), v) < minDistanceBetweenVillages / 2f));

                var isOnReservedArea = globalReservedAreas.Contains(new Vector3Int(x, y, 0));

                if (noiseVal > 1 - forestDensity && !isOnReservedArea && !isNearVillage)
                {
                    obstacleTilemap.SetTile(new Vector3Int(x, y, 0), treeTile);
                    forestReservedAreas.Add(new Vector3Int(x, y, 0));
                }
            }

            var progress = Mathf.CeilToInt(100 - (float)y / mapHeight * 100);
            statusText.text = $"Generating forests ({progress}%)...";
            yield return null;

        }

        globalReservedAreas.AddRange(allVillageReservedAreas);
        globalReservedAreas.AddRange(forestReservedAreas);

        forestGenTimer.Stop();
        Debug.Log($"<b>Forest generation</b> took <b>{forestGenTimer.ElapsedMilliseconds}</b>ms");

        var navMeshGenTimer = Stopwatch.StartNew();

        statusText.text = "Generating navigation data...";
        yield return null;

        // Bake NavMeshPlus
        yield return nms.BuildNavMeshAsync();

        navMeshGenTimer.Stop();
        Debug.Log($"<b>NavMesh generation</b> took <b>{navMeshGenTimer.ElapsedMilliseconds}</b>ms");

        statusText.text = "Finished!";

        // Stops a lag spike when animating everything at the end
        yield return new WaitForSecondsRealtime(0.15f);

        statusText.transform.parent.TweenLocalScale(Vector3.zero, 0.25f).SetEaseCubicInOut().SetOnComplete(() =>
        {
            TimeManager.instance.playButton.onClick.Invoke();

            Destroy(mapGenerationCanvas.gameObject);
            UIManager.instance.canvas.enabled = true;
            mainCanvasGroup.TweenCanvasGroupAlpha(1f, 0.25f).SetEaseCubicInOut();

            StatsManager.Reset();
            Upgrades.redeemedUpgrades.Clear();

            onFinishedLoading.Invoke();
            loadingFinished = true;
        });

        // Summon units when camera is over village, if player has that enabled
        if (game.startWithUnits)
        {
            yield return new WaitForSeconds(1f);

            // Gets the center-most village owned by the player
            var startingVillage = villages
                .Where(v => v.ownership == Ownership.Friendly)
                .OrderBy(v => Mathf.Abs(v.centerPosition.y - game.areaHeight / 2f))
                .First();

            startingVillage.PlaceSummonOrder(
                new SummonOrder(
                    startingVillage.centerPosition + new Vector3(3f, 2f),
                    new Dictionary<UnitClass, int>
                    {
                        {UnitClass.Archer, 2},
                        {UnitClass.Melee, 1}
                    },
                    UnitOwnership.Friendly
                )
            );
        }

        yield return null;
    }

    private static IEnumerable<Vector3Int> GetTilesInLine(int x0, int y0, int x1, int y1)
    {
        var tilePoints = new List<Vector3Int>();

        // Get offset between both x and y points
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2, e2;

        // Iterate over the line
        for (; ; )
        {
            tilePoints.Add(new Vector3Int(x0, y0, 1));
            if (x0 == x1 && y0 == y1) break;
            e2 = err;
            if (e2 > -dx)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dy)
            {
                err += dx;
                y0 += sy;
            }
        }

        return tilePoints;
    }
}
