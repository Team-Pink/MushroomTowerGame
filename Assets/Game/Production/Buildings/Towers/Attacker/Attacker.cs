using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Attacker
{
    public int damage = 3;

    public float attackCooldown = 3;
    public float cooldownTimer = 0f;

    public float attackDelay = 2;
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

    public Tower originReference; // I am very open to a better way of doing this so please if you can rearchitect it go ahead. 
    public Animator animator;

    [SerializeField] bool lobProjectile;

    #region TAGS
    [Header("Spray Tag")]
    public bool spray = false;
    public int sprayDamage = 1;
    public float additionalSprayRange = 2;

    [Header("Strikethrough Tag")]
    public bool strikethrough = false;
    public int strikethroughDamage = 1;
    public int strikethroughReach = 10;
    public int strikethroughBeamWidth = 4;
    public Matrix4x4 strikethroughMatrix;

    [Header("Bounce Tag")]
    public bool bounce = false;
    public int bounceHitLimit = 10;
    public bool bounceBulletTowersPossession = true;
    #endregion

    protected List<Target> targetsToShoot = new();
    public bool attacking = false;

    public virtual void Attack(HashSet<Target> targets)
    {
        Debug.LogWarning("Use one of the other methods of attacking");
    }


    protected bool CheckCooldownTimer()
    {
        if (cooldownTimer < attackCooldown + attackDelay)
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

        if (bulletPrefab == null) return;

        foreach (Target target in targetsToShoot)
        {
            if (attackParticlePrefab != null)
            {
                GameObject particle = UnityEngine.Object.Instantiate(attackParticlePrefab, transform);
                particle.transform.position += new Vector3(0, particleOriginOffset, 0);
                UnityEngine.Object.Destroy(particle, 0.5f);
            }

            Bullet bulletRef;

            bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, transform.position + Vector3.up * 2, Quaternion.identity).GetComponent<Bullet>();
            bulletRef.timeToTarget = attackDelay;
            bulletRef.target = target;
            if (lobProjectile) bulletRef.parabola = true;
            bulletRef.Initialise();
        }
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
            UnityEngine.Object.Destroy(particle, 0.5f);
        }

        Bullet bulletRef;

        bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, startingTarget.enemy.transform.position + Vector3.up * 2, Quaternion.identity).GetComponent<Bullet>();
        bulletRef.timeToTarget = timeToTarget;
        bulletRef.target = targetEnemy;
        if (lobProjectile) bulletRef.parabola = true;
        bulletRef.Initialise();
    }
    public void AnimateBounceProjectileToTower(Target targetEnemy, float timeToTarget)
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

        bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, targetEnemy.enemy.transform.position + Vector3.up * 2, Quaternion.identity).GetComponent<Bullet>();
        bulletRef.timeToTarget = timeToTarget;
        if (lobProjectile) bulletRef.parabola = true;
        bulletRef.InitialiseForNonEnemies(originReference.transform);
    }

    /// <summary>
    /// Instantiates an AttackObject assigns it's values and animates an attack.
    /// </summary>
    protected AttackObject GenerateAttackObject(Target enemy)
    {
        AttackObject attackInProgress = MonoBehaviour.Instantiate(attackObjectPrefab).GetComponent<AttackObject>();
        attackInProgress.damage = damage;
        attackInProgress.delayToTarget = attackDelay;
        attackInProgress.originTower = originReference;
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