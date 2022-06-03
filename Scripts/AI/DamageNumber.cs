using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public TextMeshPro tmp;
    public float damageValue;

    private void Start()
    {
        if(Mathf.Approximately(0, damageValue))
            Destroy(gameObject);

        tmp.text = $"{(damageValue >= 0 ? "<color=#88DD88>" : "<color=#DD8888>")} {damageValue:0.0}";

        transform.TweenLocalPositionX(transform.localPosition.x + Random.Range(-0.25f, 0.25f), 1f).SetEaseCubicOut();
        transform.TweenLocalPositionY(transform.localPosition.y + Random.Range(0.25f, 0.4f), 1f).SetEaseCubicOut();
        transform.TweenValueVector3(Vector3.zero, 1.5f, s =>
            {
                // if parent x scale is -1, flip our x scale so we display the correct way around
                transform.localScale = transform.parent.localScale.x < 0 ? new Vector3(-s.x, s.y, s.z) : s;
            })
            .SetFrom(Vector3.one)
            .SetEaseCubicOut();

        Destroy(gameObject, 1.5f);
    }
}
