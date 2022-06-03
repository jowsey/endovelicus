using System;
using System.Collections;
using System.Linq;
using ElRaccoone.Tweens;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;
using SysRandom = System.Random;

public class TowerUnit : MonoBehaviour
{
    public UnitClass unitClass;

    private Target _attackTarget;

    public Target attackTarget
    {
        get => _attackTarget;
        set
        {
            _attackTarget = value;

            if (value == null || value.ownership == UnitOwnership.Friendly) return;
            StartCoroutine(AttackLoop());
        }
    }

    public Tower tower;
    
    private float attackRange => tower.towerData.LevelValue("attackRange", tower.level);

    private float attackDamage => tower.towerData.LevelValue("attackDamage", tower.level);

    private float attackCooldown => tower.towerData.LevelValue("attackCooldown", tower.level);

    private float lastAttackTime;
    private bool attackCooldownEnded => Time.time > lastAttackTime + attackCooldown;

    [Header("Animation")]
    public Animator animator;

    private static readonly int unitOwnershipHash = Animator.StringToHash("unitOwnership");

    public SpriteRenderer spriteRenderer;

    public SpriteRenderer weaponSpriteRenderer;

    public Animator weaponAnimator;

    public Transform weaponTransform;
    

    private void Awake()
    {
        animator.enabled = false;
    }

    private void Start()
    {
        gameObject.name = unitClass.ToString();
        
        animator.SetInteger(unitOwnershipHash, (int) UnitOwnership.Friendly);
        animator.enabled = true;

        weaponSpriteRenderer.sprite = unitClass switch
        {
            UnitClass.Archer =>
                Addressables.LoadAssetAsync<Sprite>("Textures/Items/FriendlyBowBasic").WaitForCompletion(),
            UnitClass.Melee =>
                Addressables.LoadAssetAsync<Sprite>("Textures/Items/FriendlySwordBasic").WaitForCompletion(),
            _ => throw new ArgumentOutOfRangeException()
        };

        StartCoroutine(EnemyTargetingLoop());
    }

    private IEnumerator EnemyTargetingLoop()
    {
        // var rnd = new SysRandom();

        while (true)
        {
            // If we don't currently have a target
            if (attackTarget == null)
            {
                var enemiesInRadius = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Units", "Towers"))
                    .Select(c => c.GetComponent<Target>()).Where(t => t.ownership == UnitOwnership.Roman).ToList();

                if (enemiesInRadius.Count > 0)
                {
                    // Assign to unit closest
                    attackTarget = enemiesInRadius.OrderBy(u => Vector3.Distance(u.transform.position, transform.position)).First();

                    // Assign to random unit within closest half of units - good mix of targetting closest and spreading damage between group
                    // var closestHalf = enemiesInRadius
                    //     .OrderBy(u => Vector3.Distance(u.transform.position, transform.position))
                    //     .Take(Mathf.CeilToInt(enemiesInRadius.Count / 2f))
                    //     .ToList();
                    //
                    // attackTarget = closestHalf[rnd.Next(closestHalf.Count)];
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Update()
    {
        // Set Z to always be 0
        transform.position -= new Vector3(0f, 0f, transform.position.z);
    }

    private IEnumerator AttackLoop()
    {
        // While we have a target assigned
        while (attackTarget != null)
        {
            if(Vector3.Distance(attackTarget.transform.position, transform.position) <= attackRange)
            {
                // Make sure we're pointing towards the target whenever attacking
                var targetDir = attackTarget.transform.position - transform.position;
                transform.localScale = new Vector3(targetDir.x > 0 ? 1f : -1f, 1f, 1f);

                if(attackCooldownEnded)
                {
                    yield return new WaitForSeconds(Random.Range(0f, 0.2f));

                    lastAttackTime = Time.time;

                    switch (unitClass)
                    {
                        case UnitClass.Archer:
                        case UnitClass.Mage:
                            if(attackTarget == null) break;

                            weaponAnimator.enabled = false;

                            if(unitClass == UnitClass.Archer)
                            {
                                // Animate pulling bow up
                                weaponTransform.TweenLocalPositionX(0.13f, 0.4f).SetEaseCubicIn().SetPingPong();
                                weaponTransform.TweenLocalPositionY(-0.18f, 0.4f).SetEaseCubicIn().SetPingPong();

                                // Animate aiming
                                yield return new WaitForSeconds(0.15f);

                                weaponTransform.TweenLocalRotationZ(45f, 0.25f)
                                    .SetEaseCubicIn()
                                    .SetPingPong()
                                    .SetOnComplete(() => weaponAnimator.enabled = true);
                            }
                            else
                            {
                                // Animate pushing staff out
                                weaponTransform.TweenLocalPositionX(0.3f, 0.2f).SetEaseCubicIn().SetPingPong();

                                // Animate aiming
                                yield return new WaitForSeconds(0.15f);

                                weaponTransform.TweenLocalRotationZ(-25f, 0.125f)
                                    .SetEaseCubicIn()
                                    .SetPingPong()
                                    .SetOnComplete(() => weaponAnimator.enabled = true);
                            }

                            // Create projectile
                            yield return new WaitForSeconds(0.35f);

                            if(attackTarget == null) break;

                            var projectile = Addressables.InstantiateAsync("Prefabs/Weapons/Projectile")
                                .WaitForCompletion().GetComponent<Projectile>();

                            projectile.ownership = UnitOwnership.Friendly;
                            projectile.unitClass = unitClass;
                            projectile.transform.position = transform.position +
                                                            (unitClass == UnitClass.Mage ? new Vector3(0.45f * transform.localScale.x, -0.15f) : new Vector3(0f, -0.3f));

                            projectile.target = attackTarget.transform.position - new Vector3(0f, 0.25f);

                            projectile.onHitTarget.AddListener(() =>
                            {
                                if(attackTarget != null) attackTarget.health -= attackDamage;
                            });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                // if target leaves range, find new one
                attackTarget = null;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}
