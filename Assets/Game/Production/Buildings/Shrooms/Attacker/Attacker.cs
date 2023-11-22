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

    protected bool onCooldown;
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
    private readonly List<Target> hitTargets = new();
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
            if (cooldownTimer >= attackCooldown)
            {
                onCooldown = false;
                cooldownTimer = 0;
                delayTimer = 0;
            }
            else
            {
                cooldownTimer += Time.deltaTime;
                return;
            }
        }

        if (targets == null || targets.Count == 0) return;


        if (!attacking && targets.First().enemy is not null)
        {
            currentTarget = targets.First();

            attacking = true;

            AnimateAttack();

            if (bounce)
            {
                bounceBulletInShroomPossession = false;
            }

            if (lobProjectile)
            {
                targetsToShoot.Add(new Target(currentTarget.getPosition()));
            }
            else
            {
                targetsToShoot.Add(new Target(currentTarget.getPosition(), currentTarget.enemy));
            }

            if (windupParticlePrefab != null)
            {
                GameObject particle = UnityObject.Instantiate(windupParticlePrefab, transform);
                particle.transform.position += new Vector3(0, particleOriginOffset, 0);
                UnityObject.Destroy(particle, animationLeadIn);
            }
        }
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
        
        if (bounce)
        {
            originReference.boomerangCap.enabled = false;

            if (hitTargets.Count > 0)
            {
                bullet.transform.position = hitTargets.Last().getPosition();
            }
        }

        if (lobProjectile)
        {
            bullet.InitializeNoTrackParabolaBullet(currentTarget.getPosition());
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
            if (hitParticlePrefab != null) UnityObject.Instantiate(hitParticlePrefab, currentTarget.getPosition(), Quaternion.identity);
            //if (hitSoundEffect != null) AudioManager.PlaySoundEffect(hitSoundEffect.name, 1);
        }

        if (!areaAttacker)
        {
            if (currentTarget.enemy != null)
            {
                currentTarget.enemy.TakeDamage(damage);
            }

            if (bounce)
            {
                if (returning)
                {
                    returning = false;
                    originReference.boomerangCap.enabled = true;
                    bounceBulletInShroomPossession = true;
                    animator.SetBool("Attack Recoil", true);
                    hitTargets.Clear();
                }
                else
                {
                    if (currentTarget.enemy != null && currentTarget.enemy.CheckIfDead())
                    {
                        currentTarget.enemy.OnDeath();
                    }

                    hitTargets.Add(new Target (currentTarget.getPosition(), currentTarget.enemy));

                    currentTarget = FindNewBounceTarget();
                    Debug.Log("Found new target", currentTarget.enemy);

                    AnimateProjectile();
                    return;
                }
            }
        }
        else
        {
            // get everything in the area of the attack
            Collider[] mainCollisions = Physics.OverlapSphere(currentTarget.getPosition(), damageRadius, enemyLayers);
            foreach (Collider collision in mainCollisions)
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

        if (currentTarget.enemy != null && currentTarget.enemy.CheckIfDead())
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
                if (enemy.CheckIfDead()) continue;

                bool alreadyHit = false;
                foreach (Target target in hitTargets)
                {
                    if (target.enemy != null && enemy == target.enemy)
                    {
                        alreadyHit = true;
                        break;
                    }
                }
                if (alreadyHit) continue;

                if (newTarget.enemy == null)
                {
                    newTarget.enemy = enemy;
                    continue;
                }

                Vector3 lastHitPosition = hitTargets.Last().getPosition();

                float distanceToCheck = GenericUtility.FlatDistanceSqr(lastHitPosition, enemy.transform.position);
                float currentBestDistance = GenericUtility.FlatDistanceSqr(lastHitPosition, newTarget.getPosition());

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
            newTarget.position = newTarget.getPosition();
        }

        return newTarget;
    }

    HashSet<Enemy> Spray(Collider[] mainCollisions)
    {
        HashSet<Enemy> sprayTargets = new();

        foreach (Collider sprayCollision in Physics.OverlapSphere(currentTarget.getPosition(), damageRadius + additionalSprayRange, enemyLayers))
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