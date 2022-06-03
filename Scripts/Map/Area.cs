using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Area : MonoBehaviour
{
    public List<Village> villages = new List<Village>();
    public LineRenderer borderRenderer;

    public Ownership ownership { get; private set; }
    public int index;

    public UnityEvent onOwnershipChanged = new UnityEvent();

    // Start is called before the first frame update
    private void Start()
    {
        MapManager.instance.onFinishedLoading.AddListener(() =>
        {
            // Add fancy border to areas
            if(index < GameManager.instance.areaCount - 1)
            {
                borderRenderer = gameObject.AddComponent<LineRenderer>();
                borderRenderer.material = new Material(Shader.Find("Sprites/Default"));

                borderRenderer.startWidth = 0.05f;
                borderRenderer.endWidth = 0.05f;
                borderRenderer.positionCount = GameManager.instance.areaHeight / 2;

                var borderX = GameManager.instance.areaWidth * (index + 1);

                borderRenderer.SetPosition(0, new Vector3(borderX, 0, 0));
                borderRenderer.SetPosition(borderRenderer.positionCount - 1, new Vector3(borderX, GameManager.instance.areaHeight, 0));

                // shift the border a little in the middle to make it look all natural and contested
                for (var i = 1; i < borderRenderer.positionCount - 1; i++)
                {
                    borderRenderer.SetPosition(
                        i,
                        new Vector3(
                            borderX + Random.Range(-0.5f, 0.5f),
                            (float) GameManager.instance.areaHeight / borderRenderer.positionCount * i,
                            0
                        )
                    );
                }
            }

            foreach (var village in villages)
            {
                village.onOwnershipChanged.AddListener(UpdateArea);
            }

            UpdateArea();

            onOwnershipChanged.AddListener(() =>
            {
                var ownerships = MapManager.instance.areas.Select(a => a.ownership).ToList();
                if(ownerships.Distinct().Count() == 1)
                {
                    var winLoseScreen = Addressables.InstantiateAsync("Prefabs/UI/WinLoseScreen", UIManager.instance.canvas.transform)
                        .WaitForCompletion()
                        .GetComponent<WinLoseScreen>();

                    winLoseScreen.didWin = ownership == Ownership.Friendly;
                }

                if(ownership != Ownership.Roman) return;
                NotificationManager.instance.SendNotification($"Romans have taken Area {index + 1}!", Icons.ResolveIcon(Icons.SwordBrandish));
            });
        });
    }

    private void UpdateArea()
    {
        var oppositionVillages = villages.Where(v => v.ownership != ownership && v.ownership != Ownership.Neutral).ToList();

        // Friendly/Roman ownership is only swapped if all villages in area turn to a different team than the area
        // if all three villages have a different ownership to area, and there's only one different type of ownership (aka 1 team owns all villages)
        if(oppositionVillages.Count == villages.Count && oppositionVillages.Select(v => v.ownership).Distinct().Count() == 1)
        {
            var vo = villages.First().ownership;
            if(ownership != vo)
                ownership = vo;

            onOwnershipChanged.Invoke();
        }
        // Else, if the majority, but not all, of villages are of a different team, the area becomes neutral until the final village turns with them
        else if(villages.Count(v => v.ownership != ownership) > villages.Count / 2 && ownership != Ownership.Neutral)
        {
            ownership = Ownership.Neutral;
            onOwnershipChanged.Invoke();
        }

        // Update border colours
        if(index > 0)
        {
            // Border colour is based on the ownership of the area to the right, so we set the colour of the area to our left
            var reducedOpacityColour = ownership switch
            {
                Ownership.Friendly => Constants.friendlyColour,
                Ownership.Roman => Constants.enemyColour,
                Ownership.Neutral => Constants.neutralColour,
                _ => throw new ArgumentOutOfRangeException()
            };

            var leftArea = MapManager.instance.areas.First(a => a.index == index - 1);

            reducedOpacityColour.a = 0.333f;
            leftArea.borderRenderer.startColor = reducedOpacityColour;
            leftArea.borderRenderer.endColor = reducedOpacityColour;
        }
    }
}
