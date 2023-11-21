using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

[Serializable]
public class Attacker
{
    public int damage = 3;

    public bool areaAttacker;

    private bool onCooldown;
    public float attackCooldown = 3;
    public float cooldownTimer = 0f;

    
    public float attackDelay = 2; // this is the time it takes for an attack to reach a target.
    protected float delayTimer = 0f;

    public float animationLeadIn = 0f;

    public Transform transform;
    public GameObject bulletPrefab;
    public GameObject attackObjectPrefab;

    public GameObject windupParticlePrefab;
    public GameObject attackParticlePrefab;
    public GameObject hitParticlePrefab;
    public float particleOriginOffset = 0.0f;

    public AudioClip leadinSoundEffect;
    public AudioClip attackSoundEffect;
    public AudioClip attackHitSoundEffect;

    public Shroom originReference;
    public Animator animator;

    [SerializeField]protected  bool lobProjectile;
    

    [Header("Spray Tag")]
    public bool spray = false;
    public int sprayDamage = 1;
    public float additionalSprayRange = 2;

    [Header("Bounce Tag")]
    public bool bounce = false;
    public int bounceHitLimit = 10;
    public bool returning = false;
    private readonly List<Enemy> hitEnemies = new();
    [HideInInspector] public bool bounceBulletInShroomPossession = true;

    [Space()]
    public LayerMask enemyLayers;
    public float damageRadius = 3f;
    protected List<Target> targetsToShoot = new();
    public bool attacking = false;
    protected Target currentTarget;

    public virtual void Attack(HashSet<Target> targets)
    {
        if (onCooldown)
        {
            if (CheckAndIncrementCooldown() == false)
            {
                return;
            }
            else
            {
                onCooldown = false;
                cooldownTimer = 0;
                delayTimer = 0;
            }
        }

        if (targets == null || targets.Count == 0) return;

        currentTarget = targets.First();

        if (!attacking && currentTarget.enemy is not null)
        {
            attacking = true;

            AnimateAttack();

            if (bounce)
            {
                bounceBulletInShroomPossession = false;
            }

            if (lobProjectile)
            {
                targetsToShoot.Add(new Target(currentTarget.position));
            }
            else
            {
                targetsToShoot.Add(currentTarget);
            }

            if (windupParticlePrefab != null)
            {
                GameObject particle = UnityObject.Instantiate(windupParticlePrefab, transform);
                particle.transform.position += new Vector3(0, particleOriginOffset, 0);
                UnityObject.Destroy(particle, animationLeadIn);
            }
        }
    }


    public bool CheckAndIncrementCooldown()
    {
        if (cooldownTimer < attackCooldown)
        {
            cooldownTimer += Time.deltaTime;
            return false;
        }
        return true;
    }

    public void AnimateProjectile()
    {
        if (attackSoundEffect != null)
        {
            AudioManager.PlaySoundEffect(attackSoundEffect.name, 1);
        }

        if (attackParticlePrefab != null)
        {
            GameObject particle = UnityObject.Instantiate(attackParticlePrefab, transform);
            particle.transform.position += new Vector3(0, particleOriginOffset, 0);
            UnityObject.Destroy(particle, 0.5f);
        }

        Debug.Log("Bullet fired " + attackDelay);

        Bullet bullet;
        if (bulletPrefab != null)
        {
            bullet = UnityObject.Instantiate(bulletPrefab, transform.position + Vector3.up * 2, Quaternion.identity).GetComponent<Bullet>();
        }
        else
        {
            GameObject bulletObject;
            bulletObject = new GameObject();
            bulletObject.transform.position = transform.position + Vector3.up * 2;
            bullet = bulletObject.GetComponent<Bullet>();
        }

        bullet.timeToTarget = attackDelay;
        bullet.target = currentTarget;
        bullet.attacker = this;

        if (bounce && hitEnemies.Count > 0)
        {
            Vector3 lastHitPosition = hitEnemies.Last().transform.position;
            bullet.transform.position = new Vector3( lastHitPosition.x, transform.position.y + 2, lastHitPosition.z);
        }

        if (lobProjectile)
        {
            bullet.InitializeNoTrackParabolaBullet(currentTarget.position);
        }
        else
        {
            if (currentTarget.enemy)
            {
                bullet.Initialise();
            }
            else
            {
                bullet.InitialiseForTargetPosition(originReference.transform.position);
            }
        }

        targetsToShoot.Clear();
    }

    public void AttackHit()
    {
        // play impact animation
        if (currentTarget.enemy != null)
        {
            if (hitParticlePrefab != null) UnityObject.Instantiate(hitParticlePrefab, currentTarget.position, Quaternion.identity);
            //if (hitSoundEffect != null) AudioManager.PlaySoundEffect(hitSoundEffect.name, 1);
        }

        if (!areaAttacker)
        {
            currentTarget.enemy.TakeDamage(damage);

            if (bounce)
            {
                if (returning)
                {
                    returning = false;
                }
                else
                {
                    if (currentTarget.enemy.CheckIfDead())
                    {
                        currentTarget.enemy.OnDeath();
                    }

                    hitEnemies.Add(currentTarget.enemy);

                    currentTarget = FindNewBounceTarget();

                    AnimateProjectile();
                    return;
                }
            }
        }
        else
        {
            // get everything in the area of the attack
            Collider[] mainCollisions = Physics.OverlapSphere(currentTarget.position, damageRadius, enemyLayers);
            foreach (var collision in mainCollisions)
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy is null) continue;

                enemy.TakeDamage(damage);
                if (enemy.CheckIfDead())
                {
                    if (enemy != currentTarget.enemy && enemy.CheckIfDead())
                    {
                        enemy.OnDeath();
                    }
                }
            }

            if (spray)
            {
                HashSet<Enemy> secondaryEnemies = Spray(mainCollisions);

                foreach (Enemy enemy in secondaryEnemies)
                {
                    enemy.TakeDamage(sprayDamage);
                }
            }
        }

        onCooldown = true;
        attacking = false;

        if (currentTarget.enemy.CheckIfDead())
        {
            currentTarget.enemy.OnDeath();
        }
    }

    private Target FindNewBounceTarget()
    {
        Collider[] potentialTargets = Physics.OverlapSphere(originReference.transform.position, originReference.TargeterComponent.range, enemyLayers);

        Target newTarget = new();

        foreach (Collider potentialTarget in potentialTargets)
        {
            if (potentialTarget.gameObject.TryGetComponent(out Enemy enemy))
            {
                if (hitEnemies.Contains(enemy) || enemy.CheckIfDead()) continue;

                if (newTarget.enemy == null)
                {
                    newTarget.enemy = enemy;
                    continue;
                }

                Vector3 lastHitPosition = hitEnemies.Last().transform.position;

                float distanceToCheck = GenericUtility.FlatDistanceSqr(lastHitPosition, enemy.transform.position);
                float currentBestDistance = GenericUtility.FlatDistanceSqr(lastHitPosition, newTarget.enemy.transform.position);

                if (distanceToCheck < currentBestDistance) newTarget.enemy = enemy;
            }
        }

        if (newTarget.enemy == null)
        {
            newTarget.position = originReference.transform.position;
            returning = true;
        }
        else
        {
            newTarget.position = newTarget.enemy.transform.position;
        }

        return newTarget;
    }

    HashSet<Enemy> Spray(Collider[] mainCollisions)
    {
        HashSet<Enemy> sprayTargets = new();

        foreach (Collider sprayCollision in Physics.OverlapSphere(currentTarget.position, damageRadius + additionalSprayRange, enemyLayers))
        {
            bool isMainCollision = false;
            Enemy enemy = sprayCollision.gameObject.GetComponent<Enemy>();

            foreach (Collider mainCollision in mainCollisions)
            {
                if (mainCollision == sprayCollision)
                {
                    isMainCollision = true;
                    break;
                }
            }

            if (!isMainCollision) sprayTargets.Add(enemy);
        }
        return sprayTargets;
    }

    public void AnimateBounceProjectileToEnemy(Target startingTarget, Target targetEnemy, float timeToTarget)
    {
        if (attackSoundEffect != null)
        {
            AudioManager.PlaySoundEffect(attackSoundEffect.name, 1);
        }

        if (bulletPrefab == null) return;

        if (attackParticlePrefab != null)
        {
            GameObject particle = UnityEngine.Object.Instantiate(attackParticlePrefab, transform);
            particle.transform.position += new Vector3(0, particleOriginOffset, 0);
            UnityObject.Destroy(particle, 0.5f);
        }

        Bullet bulletRef;

        bulletRef = UnityObject.Instantiate(bulletPrefab, startingTarget.enemy.transform.position, Quaternion.identity).GetComponent<Bullet>();
        bulletRef.timeToTarget = timeToTarget;
        bulletRef.target = targetEnemy;
        bulletRef.attacker = this;
        bulletRef.Initialise();
    }
    public void AnimateBounceProjectileToShroom(Target targetEnemy, float timeToTarget)
    {
        if (attackSoundEffect != null)
        {
            AudioManager.PlaySoundEffect(attackSoundEffect.name, 1);
        }

        if (bulletPrefab == null) return;

        if (attackParticlePrefab != null)
        {
            GameObject particle = UnityEngine.Object.Instantiate(attackParticlePrefab, transform);
            particle.transform.position += new Vector3(0, particleOriginOffset, 0);
            UnityEngine.Object.Destroy(particle, 0.5f);
        }

        Bullet bulletRef;
        if (targetEnemy.enemy != null)
            bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, targetEnemy.enemy.transform.position, Quaternion.identity).GetComponent<Bullet>();
        else
            bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, originReference.transform.position, Quaternion.identity).GetComponent<Bullet>();
        bulletRef.timeToTarget = timeToTarget;
        if (lobProjectile) bulletRef.parabola = true;
        bulletRef.InitialiseForTargetPosition(originReference.transform.position + Vector3.up * 2);
    }

    /// <summary>
    /// Instantiates an AttackObject assigns it's universal values.
    /// </summary>
    protected AttackObject GenerateAttackObject(Target enemy)
    {
        AttackObject attackInProgress = MonoBehaviour.Instantiate(attackObjectPrefab).GetComponent<AttackObject>();
        attackInProgress.damage = damage;
        attackInProgress.delayToTarget = attackDelay;
        attackInProgress.originShroom = originReference;
        attackInProgress.target = enemy;
        return attackInProgress;
    }

    public void AnimateAttack()
    {
        if (leadinSoundEffect != null)
        {
            AudioManager.PlaySoundEffect(leadinSoundEffect.name, 1, animationLeadIn);
        }

        if (animator == null) return;

        animator.SetTrigger("Attack");       
    }
}