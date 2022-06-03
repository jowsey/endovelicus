using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class FogManager : MonoBehaviour
{
    public static FogManager instance { get; private set; }
    public Tilemap tilemap;

    private RuleTile fogTile;
    public UnityEvent onFogUpdate = new UnityEvent();

    public int fogBeginX;

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

        fogTile = Addressables.LoadAssetAsync<RuleTile>("Tiles/Fog").WaitForCompletion();
    }

    private void Start()
    {
        MapManager.instance.onFinishedLoading.AddListener(() =>
        {
            foreach (var area in MapManager.instance.areas)
            {
                area.onOwnershipChanged.AddListener(() =>
                {
                    StopCoroutine(UpdateAreaLighting());
                    StartCoroutine(UpdateAreaLighting());
                });
            }
        });
    }

    private IEnumerator UpdateAreaLighting()
    {
        var areas = MapManager.instance.areas;
        var IndexToBeginFog = areas.FindIndex(a => a.ownership != Ownership.Friendly) + 1;

        var fogTopLeft = new Vector2Int(IndexToBeginFog * GameManager.instance.areaWidth, GameManager.instance.areaHeight);
        var fogBottomRight = new Vector2Int(areas.Count * GameManager.instance.areaWidth, 0);

        // don't do the whole process if nothing's changed since last update
        if(fogBeginX == fogTopLeft.x) yield break;

        fogBeginX = fogTopLeft.x;
        onFogUpdate.Invoke();

        // Remove tiles from everywhere before the fog
        for (var x = -1; x < fogTopLeft.x; x++)
        {
            for (var y = -1; y <= fogTopLeft.y; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }

            yield return new WaitForEndOfFrame();
        }

        // Set fog to everywhere unowned
        for (var x = fogBottomRight.x; x >= fogTopLeft.x; x--)
        {
            for (var y = fogBottomRight.y - 1; y <= fogTopLeft.y; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), fogTile);
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
