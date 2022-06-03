using System.Collections;
using System.Collections.Generic;
using ElRaccoone.Tweens;
using UnityEngine;

public class HealingRingObject : MonoBehaviour
{
    public float duration;
    public float healingPerSecond;

    private const float tickRate = 4f;

    public List<Unit> units = new List<Unit>();

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        // animate between 100% and 50% opacity
        gameObject.TweenSpriteRendererColor(Color.white - new Color(0f, 0f, 0f, 0.25f), 0.25f).SetEaseCubicInOut().SetPingPong().SetInfinite();

        var elapsedTime = 0f;
        while (elapsedTime <= duration)
        {
            foreach (var unit in units)
            {
                // unit might've died in-between colliding and healing
                if (unit != null)
                {
                    unit.health += healingPerSecond / tickRate;
                }
            }

            elapsedTime += 1f / tickRate;
            yield return new WaitForSeconds(1f / tickRate);
        }

        gameObject.TweenCancelAll();
        gameObject.TweenSpriteRendererColor(Color.clear, 1f).SetEaseCubicInOut().SetOnComplete(() => Destroy(gameObject));

        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent(out Unit unit)) return;

        if (unit.ownership == UnitOwnership.Friendly)
        {
            units.Add(unit);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.TryGetComponent(out Unit unit)) return;

        units.Remove(unit);
    }
}
