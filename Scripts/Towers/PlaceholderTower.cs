using System.Linq;
using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;

public class PlaceholderTower : MonoBehaviour
{
    private Vector3Int _position;

    public Vector3Int position
    {
        get => _position;
        set
        {
            _position = value;
            CheckPositionsAreFree();

            gameObject.TweenPosition(value, 0.25f)
                .SetEaseCubicOut()
                .SetUseUnscaledTime(true);
        }
    }

    public TowerData towerData;
    public GameObject sprite;
    public LineRenderer borderRenderer;

    public TextMeshProUGUI placementReasoning;

    private void Start()
    {
        var radius = towerData.LevelValue("attackRange", 1);

        // Set the positions of the border renderer
        for (var i = 0; i < borderRenderer.positionCount; i++)
        {
            var angle = i * Mathf.PI * 2 / borderRenderer.positionCount;
            borderRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0) + new Vector3(1f, 1f));
        }
    }

    public bool CheckPositionsAreFree()
    {
        var friendlyVillagePositions = MapManager.instance.villages
        .Where(v => v.ownership == Ownership.Friendly)
        .Select(v => v.centerPosition);

        var isCloseToFriendly = friendlyVillagePositions.Any(p => Vector3.Distance(position, p) < 16f);

        for (var x = 0; x <= 1; x++)
        {
            for (var y = 0; y <= 1; y++)
            {
                var isInReservedArea = MapManager.instance.globalReservedAreas.Contains(position + new Vector3Int(x, y, -position.z));

                if (!isInReservedArea && isCloseToFriendly)
                {
                    placementReasoning.text = "<color=#3FFF3F>Valid position!</color>";
                    continue;
                }

                placementReasoning.text = $"<color=#FF3F3F>{(isInReservedArea ? "Is over reserved area! (trees, building etc)" : "")}\n{(!isCloseToFriendly ? "Is too far away from a friendly area!" : "")}";

                sprite.TweenSpriteRendererColor(new Color(1f, 0.25f, 0.25f, 1f), 0.25f)
                    .SetEaseCubicInOut()
                    .SetUseUnscaledTime(true);

                return false;
            }
        }

        sprite.TweenSpriteRendererColor(new Color(0.25f, 1f, 0.25f, 1f), 0.25f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true);

        return true;
    }

    public void End()
    {
        borderRenderer.enabled = false;

        sprite.TweenSpriteRendererColor(Color.clear, 1f)
            .SetEaseCubicInOut()
            .SetUseUnscaledTime(true)
            .SetOnComplete(() => Destroy(gameObject));
    }
}
