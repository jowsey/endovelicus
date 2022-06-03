using ElRaccoone.Tweens;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public UnitOwnership ownership;

    public Vector3 target;
    private const float travelTime = 0.3f;

    public SpriteRenderer sr;

    public UnitClass unitClass;

    [HideInInspector]
    public UnityEvent onHitTarget = new UnityEvent();

    // Start is called before the first frame update
    private void Start()
    {
        sr.sprite = ownership switch
        {
            UnitOwnership.Friendly when unitClass == UnitClass.Archer => Addressables.LoadAssetAsync<Sprite>("Textures/Items/FriendlyArrowBasic").WaitForCompletion(),
            UnitOwnership.Roman when unitClass == UnitClass.Archer => Addressables.LoadAssetAsync<Sprite>("Textures/Items/RomanArrowBasic").WaitForCompletion(),

            UnitOwnership.Friendly when unitClass == UnitClass.Mage => Addressables.LoadAssetAsync<Sprite>("Textures/Items/FriendlyMagicBasic").WaitForCompletion(),
            UnitOwnership.Roman when unitClass == UnitClass.Mage => Addressables.LoadAssetAsync<Sprite>("Textures/Items/RomanMagicBasic").WaitForCompletion(),
            _ => null
        };

        // make the magic projectile do the spinny
        if(unitClass == UnitClass.Mage)
            transform.TweenLocalRotationZ(360f, travelTime).SetEaseCubicIn();

        transform.TweenValueVector3(target, travelTime, val =>
            {
                transform.transform.position = val;
                transform.right = target - transform.position;
            })
            .SetFrom(transform.position)
            .SetEaseExpoIn()
            .SetOnComplete(() =>
            {
                SoundManager.PlayShortRanged(SoundManager.GetAudioClip(unitClass == UnitClass.Archer ? "SFX/ArrowHit" : "SFX/MagicHit"), transform.position);
                onHitTarget.Invoke();
                Destroy(gameObject);
            });
    }
}
