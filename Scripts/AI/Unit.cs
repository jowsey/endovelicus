using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ElRaccoone.Tweens;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using SysRandom = System.Random;

public enum UnitOrder
{
    // Standing still, no orders given
    Idle,

    // Move directly to location without attacking, ie when player right click moves
    DirectMove,

    // Move and attack any enemies along way, ie when player shift+right click moves, or AI orders
    AttackMove,
}

public enum UnitOwnership
{
    Friendly,
    Roman
}

public enum UnitClass
{
    Melee,
    Archer,
    Mage
}

public static class UnitStats
{
    public enum UnitStat
    {
        MaxHealth,
        AttackCooldown,
        AttackRange,
        AttackDamage,
    }

    public static float GetModifiedStat(Unit unit, UnitStat stat) =>
        baseStats[unit.unitClass][stat] * (unit.ownership == UnitOwnership.Friendly ? modifiers[unit.unitClass][stat] : 1f);

    public static float GetModifiedStat(TowerUnit unit, UnitStat stat) => baseStats[unit.unitClass][stat] * modifiers[unit.unitClass][stat];

    public static readonly IDictionary<UnitClass, IDictionary<UnitStat, float>> baseStats = new Dictionary<UnitClass, IDictionary<UnitStat, float>>
    {
        {
            UnitClass.Melee, new Dictionary<UnitStat, float>
            {
                {UnitStat.MaxHealth, 22f},
                {UnitStat.AttackCooldown, 2f},
                {UnitStat.AttackRange, 0.75f},
                {UnitStat.AttackDamage, 4f}
            }
        },
        {
            UnitClass.Archer, new Dictionary<UnitStat, float>
            {
                {UnitStat.MaxHealth, 18f},
                {UnitStat.AttackCooldown, 1.5f},
                {UnitStat.AttackRange, 8f},
                {UnitStat.AttackDamage, 2.5f}
            }
        },
        {
            UnitClass.Mage, new Dictionary<UnitStat, float>
            {
                {UnitStat.MaxHealth, 22f},
                {UnitStat.AttackCooldown, 0.6f},
                {UnitStat.AttackRange, 10f},
                {UnitStat.AttackDamage, 4f}
            }
        },

    };

    public static readonly IDictionary<UnitClass, IDictionary<UnitStat, float>> modifiers = new Dictionary<UnitClass, IDictionary<UnitStat, float>>
    {
        {
            UnitClass.Melee, new Dictionary<UnitStat, float>
            {
                {UnitStat.MaxHealth, 1f},
                {UnitStat.AttackCooldown, 1f},
                {UnitStat.AttackRange, 1f},
                {UnitStat.AttackDamage, 1f}
            }
        },
        {
            UnitClass.Archer, new Dictionary<UnitStat, float>
            {
                {UnitStat.MaxHealth, 1f},
                {UnitStat.AttackCooldown, 1f},
                {UnitStat.AttackRange, 1f},
                {UnitStat.AttackDamage, 1f}
            }
        },
        {
            UnitClass.Mage, new Dictionary<UnitStat, float>
            {
                {UnitStat.MaxHealth, 1f},
                {UnitStat.AttackCooldown, 1f},
                {UnitStat.AttackRange, 1f},
                {UnitStat.AttackDamage, 1f}
            }
        }
    };
}

[RequireComponent(typeof(Target))]
public class Unit : MonoBehaviour
{
    [Header("Gameplay")]
    public UnitClass unitClass;

    public UnitOwnership ownership { get; set; }

    public AIUnitGroup unitGroup;

    private Vector3 _mainDestination;

    public Vector3 mainDestination
    {
        get => _mainDestination;
        set
        {
            _mainDestination = value;

            if(navMeshAgent != null)
                navMeshAgent.SetDestination(value);
        }
    }

    private NavMeshPath _mainPath;

    public NavMeshPath mainPath
    {
        get => _mainPath;
        set
        {
            _mainPath = value;

            if(navMeshAgent != null)
                navMeshAgent.SetPath(value);

            mainDestination = value.corners.LastOrDefault();
        }
    }

    private UnitOrder _orderType = UnitOrder.Idle;

    public UnitOrder orderType
    {
        get => _orderType;
        set
        {
            _orderType = value;
            lineRenderer.startColor = lineRenderer.endColor = (value == UnitOrder.DirectMove ? Constants.directMoveColour : Constants.attackMoveColour);

            // Remove attack target if new order doesn't allow attacking
            if(value == UnitOrder.DirectMove && attackTarget != null)
            {
                attackTarget = null;
            }
        }
    }

    public Target target;

    private Target _attackTarget;

    public Target attackTarget
    {
        get => _attackTarget;
        set
        {
            _attackTarget = value;

            if(value == null || value.ownership == ownership) return;
            StartCoroutine(AttackLoop());
        }
    }

    private float _health;

    public float health
    {
        get => _health;
        set
        {
            if(value <= 0f)
            {
                Die();
            }

            value = Mathf.Clamp(value, 0f, maxHealth);

            // if we die in the same frame as we take damage from something else, it'll try and add a
            // damage number to something which now doesn't exist, so we check
            if(transform != null)
            {
                var diff = value - _health;
                Addressables.InstantiateAsync("Prefabs/Units/DamageNumber", transform)
                    .WaitForCompletion()
                    .GetComponent<DamageNumber>().damageValue = diff;
            }

            _health = value;

            selectionRing.color = Constants.redGreenGradient.Evaluate(health / maxHealth);
        }
    }

    public float maxHealth => UnitStats.GetModifiedStat(this, UnitStats.UnitStat.MaxHealth);

    private float attackRange => UnitStats.GetModifiedStat(this, UnitStats.UnitStat.AttackRange);

    private float attackDamage => UnitStats.GetModifiedStat(this, UnitStats.UnitStat.AttackDamage);

    private float attackCooldown => UnitStats.GetModifiedStat(this, UnitStats.UnitStat.AttackCooldown);

    private float lastAttackTime;
    private bool attackCooldownEnded => Time.time > lastAttackTime + attackCooldown;

    [Header("Animation")]
    public Animator animator;

    private static readonly int unitOwnershipHash = Animator.StringToHash("unitOwnership");

    public SpriteRenderer spriteRenderer;

    public SpriteRenderer weaponSpriteRenderer;

    public Animator weaponAnimator;

    public Transform weaponTransform;

    public List<AudioClip> footstepNoises;

    [Header("Other")]
    public LineRenderer lineRenderer;

    public NavMeshAgent navMeshAgent;
    private static readonly int isMovingHash = Animator.StringToHash("isMoving");

    public GameObject selectionDisplay;

    public SpriteRenderer selectionRing;

    public bool selected
    {
        get => selectionDisplay.activeSelf;
        set
        {
            selectionDisplay.SetActive(value);

            if(value)
            {
                selectionRing.transform.TweenRotationZ(180f, 15f).SetUseUnscaledTime(true).SetLoopCount(-1);
            }
            else
            {
                // LeanTween.cancel(selectionPointer.gameObject);
                selectionRing.transform.gameObject.TweenCancelAll();
            }
        }
    }

    private bool isMoving;

    private void Awake()
    {
        health = maxHealth;

        animator.enabled = false;
    }

    private void Start()
    {
        gameObject.name = $"{ownership} {unitClass}";

        target.unit = this;

        // animator.SetInteger(unitClassHash, (int)unitClass);
        animator.SetInteger(unitOwnershipHash, (int) ownership);
        animator.runtimeAnimatorController = unitClass switch
        {
            UnitClass.Archer => Addressables.LoadAssetAsync<RuntimeAnimatorController>("Animation/ArcherController").WaitForCompletion(),
            UnitClass.Melee => Addressables.LoadAssetAsync<RuntimeAnimatorController>("Animation/MeleeController").WaitForCompletion(),
            UnitClass.Mage => Addressables.LoadAssetAsync<RuntimeAnimatorController>("Animation/MageController").WaitForCompletion(),
            _ => null
        };

        navMeshAgent.updateRotation = navMeshAgent.updateUpAxis = false;
        animator.enabled = true;

        weaponSpriteRenderer.sprite = unitClass switch
        {
            UnitClass.Melee when ownership == UnitOwnership.Friendly =>
                Addressables.LoadAssetAsync<Sprite>("Textures/Items/FriendlySwordBasic").WaitForCompletion(),
            UnitClass.Archer when ownership == UnitOwnership.Friendly =>
                Addressables.LoadAssetAsync<Sprite>("Textures/Items/FriendlyBowBasic").WaitForCompletion(),
            UnitClass.Mage when ownership == UnitOwnership.Friendly =>
                Addressables.LoadAssetAsync<Sprite>("Textures/Items/FriendlyStaffBasic").WaitForCompletion(),

            UnitClass.Melee when ownership == UnitOwnership.Roman =>
                Addressables.LoadAssetAsync<Sprite>("Textures/Items/RomanSwordBasic").WaitForCompletion(),
            UnitClass.Archer when ownership == UnitOwnership.Roman =>
                Addressables.LoadAssetAsync<Sprite>("Textures/Items/RomanBowBasic").WaitForCompletion(),
            UnitClass.Mage when ownership == UnitOwnership.Roman =>
                Addressables.LoadAssetAsync<Sprite>("Textures/Items/RomanStaffBasic").WaitForCompletion(),
            _ => null
        };

        // Assign footstep noises to moving animations
        var footstepEvent = new AnimationEvent
        {
            time = 0.8f,
            functionName = "Footstep"
        };

        foreach (var clip in animator.runtimeAnimatorController!.animationClips.Where(c => c.name.EndsWith("Moving") && c.events.Length == 0))
        {
            clip.AddEvent(footstepEvent);
        }

        StartCoroutine(EnemyTargetingLoop());
    }

    [UsedImplicitly]
    private void Footstep()
    {
        SoundManager.PlayShortRanged(footstepNoises[Random.Range(0, footstepNoises.Count)], transform.position);
    }

    // Is visible if friendly, or, when roman, if not in fog
    private bool CheckFogVisibility()
    {
        var isVisible = transform.position.x < FogManager.instance.fogBeginX;

        spriteRenderer.enabled = weaponSpriteRenderer.enabled = (ownership == UnitOwnership.Friendly) || isVisible;
        return isVisible;
    }

    private IEnumerator EnemyTargetingLoop()
    {
        var rnd = new SysRandom();

        while (true)
        {
            // If we don't currently have a target, and have an order that allows us to fight, search for targets
            if(attackTarget == null && (orderType == UnitOrder.AttackMove || orderType == UnitOrder.Idle))
            {
                var enemiesInRadius = Physics2D.OverlapCircleAll(transform.position, 12f, LayerMask.GetMask("Units", "Towers"))
                    .Select(c => c.GetComponent<Target>()).Where(t => t.ownership != ownership).ToList();

                if(enemiesInRadius.Count > 0)
                {
                    // Assign to unit closest
                    // attackTarget = enemiesInRadius.OrderBy(u => Vector3.Distance(u.transform.position, transform.position)).First();

                    // Assign to random unit within closest half of units - good mix of targetting closest and spreading damage between group
                    var closestHalf = enemiesInRadius
                        .OrderBy(u => Vector3.Distance(u.transform.position, transform.position))
                        .Take(Mathf.CeilToInt(enemiesInRadius.Count / 2f))
                        .ToList();

                    attackTarget = closestHalf[rnd.Next(closestHalf.Count)];
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Update()
    {
        // Set isMoving bool based on if nav mesh agent is in motion
        isMoving = navMeshAgent.velocity.magnitude >= 0.1f && navMeshAgent.desiredVelocity.magnitude >= 0.1f;

        if(isMoving != animator.GetBool(isMovingHash))
        {
            animator.SetBool(isMovingHash, isMoving);
            weaponAnimator.SetBool(isMovingHash, isMoving);
        }

        // Flip sprites on X axis based on if moving left or right
        if(isMoving && weaponAnimator.enabled)
        {
            transform.localScale = new Vector3(navMeshAgent.velocity.x > 0 ? 1f : -1f, 1f, 1f);
        }

        // Set Z to always be 0
        transform.position -= new Vector3(0f, 0f, transform.position.z);

        // Update LineRenderer if selected
        lineRenderer.enabled = selected && navMeshAgent.velocity.magnitude >= 1f;
        if(selected)
        {
            var corners = navMeshAgent.path.corners;

            lineRenderer.positionCount = corners.Length;
            lineRenderer.SetPositions(corners.Select(c => c + new Vector3(0f, -0.5f, 0.5f)).ToArray());
        }
    }

    private void LateUpdate()
    {
        // Animations were based on Bows, which means other classes need a little adjustment
        // There's probably a nicer way around this, but it works fine and I'm low on time so we ball
        if(unitClass == UnitClass.Melee)
        {
            var lp = weaponTransform.localPosition;
            lp.x += 0.19f;
            lp.y += 0.15f;
            weaponTransform.localPosition = lp;
        }
        else if(unitClass == UnitClass.Mage && weaponAnimator.enabled)
        {
            var lp = weaponTransform.localPosition;
            lp.x += 0.14f;
            lp.y += 0.08f;
            weaponTransform.localPosition = lp;
        }

        // Set order to idle when navmesh reaches destination
        if(Vector3.Distance(mainDestination, transform.position) < navMeshAgent.stoppingDistance && !isMoving && orderType != UnitOrder.Idle)
        {
            orderType = UnitOrder.Idle;
        }

        if(FogManager.instance != null)
        {
            var isVisible = CheckFogVisibility();

            // If in fog and friendly, retreat back to nearest village in friendly area
            if(!isVisible && ownership == UnitOwnership.Friendly)
            {
                orderType = UnitOrder.DirectMove;
                mainDestination = MapManager.instance.villages
                    .Where(v => v.area.ownership == Ownership.Friendly)
                    .OrderBy(v => Vector3.Distance(v.centerPosition, transform.position))
                    .First().centerPosition;
            }
        }
    }

    private IEnumerator AttackLoop()
    {
        // While we have a target assigned
        while (attackTarget != null)
        {
            if(Vector3.Distance(attackTarget.transform.position, transform.position) > attackRange + (attackTarget.targetType == TargetType.Tower ? 1f : 0f))
            {
                if(navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.SetDestination(attackTarget.transform.position);
                    navMeshAgent.stoppingDistance = attackRange;
                }
            }
            else
            {
                // If we're close enough to the enemy, we can stop moving towards them
                if(navMeshAgent.hasPath)
                {
                    navMeshAgent.ResetPath();
                }

                // Make sure we're pointing towards the target whenever attacking
                var targetDir = transform.TransformDirection(attackTarget.transform.position - transform.position);
                transform.localScale = new Vector3(targetDir.x > 0 ? 1f : -1f, 1f, 1f);

                if(attackCooldownEnded)
                {
                    lastAttackTime = Time.time;
                    
                    yield return new WaitForSeconds(Random.Range(0f, 0.2f));

                    switch (unitClass)
                    {
                        case UnitClass.Melee:
                            // if either unit has died in-between queuing the attack and doing it
                            if(attackTarget == null || target == null) break;

                            SoundManager.PlayShortRanged(SoundManager.GetAudioClip("SFX/SwordHit" + Random.Range(1, 5)), transform.position);

                            // Animate lunging towards opponent
                            transform.TweenPosition(Vector3.Lerp(transform.position, attackTarget.transform.position, 0.33f), 0.15f)
                                .SetEaseCubicIn()
                                .SetPingPong();

                            // Animate swinging sword
                            weaponTransform.TweenLocalRotationZ(-60f, 0.15f)
                                .SetEaseCubicIn()
                                .SetPingPong()
                                .SetOnComplete(() =>
                                {
                                    if(attackTarget != null) attackTarget.health -= attackDamage;
                                });
                            break;
                        case UnitClass.Archer:
                        case UnitClass.Mage:
                            if(attackTarget == null || target == null) break;
                            
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
                            yield return new WaitForSeconds(0.3f);

                            if(attackTarget == null) break;

                            var projectile = Addressables.InstantiateAsync("Prefabs/Weapons/Projectile")
                                .WaitForCompletion()
                                .GetComponent<Projectile>();

                            projectile.ownership = ownership;
                            projectile.unitClass = unitClass;
                            projectile.transform.position = transform.position +
                                                            (unitClass == UnitClass.Mage
                                                                ? new Vector3(0.45f * transform.localScale.x, -0.15f)
                                                                : new Vector3(0f, -0.3f)
                                                            );

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

            yield return new WaitForSeconds(0.2f);
        }

        // Enemy is null/destroyed
        if(mainDestination != Vector3.zero && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(mainDestination);
        }
    }

    private void Die()
    {
        // become corpse, can no longer be used, targeted, etc
        StatsManager.stats[ownership == UnitOwnership.Friendly ? StatType.FriendlyUnitsLost : StatType.RomanUnitsKilled]++;

        unitGroup?.RemoveUnit(this);
        DragSelect.selectedUnits.Remove(this);

        if(ownership == UnitOwnership.Roman && LevelManager.instance != null)
        {
            LevelManager.instance.xp += 1;
        }

        Destroy(this);
        Destroy(target);

        Destroy(selectionDisplay);
        Destroy(animator);
        Destroy(weaponAnimator.gameObject);
        Destroy(navMeshAgent);

        var go = gameObject;
        go.layer = LayerMask.NameToLayer("Default");
        go.name = $"{ownership} Corpse";
        spriteRenderer.sortingOrder = -1;

        // flop dead x_x
        var ls = transform.localScale;
        var pos = transform.position;

        gameObject.TweenCancelAll();
        transform.TweenRotationZ(Random.Range(85f, 95f) * ls.x, 0.25f).SetEaseCubicIn();
        transform.TweenLocalPositionX(pos.x + (-0.5f * ls.x), 0.25f).SetEaseCubicIn();
        transform.TweenLocalPositionY(pos.y + 0.25f, 0.25f).SetEaseCubicIn().SetPingPong();

        // fade away
        go.TweenSpriteRendererColor(new Color(0.5f, 0.25f, 0.25f, 0.3f), 0.5f)
            .SetEaseCubicIn()
            .SetOnComplete(() => go.TweenSpriteRendererColor(Color.clear, 15f).SetEaseCubicIn().SetOnComplete(() => Destroy(go)));
    }
}
