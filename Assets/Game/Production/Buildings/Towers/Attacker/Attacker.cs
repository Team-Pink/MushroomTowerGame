using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Attacker
{
    public int damage = 3;

    public float attackCooldown = 3;
    protected float cooldownTimer = 0f;

    public float attackDelay = 2;
    protected float delayTimer = 0f;

    public float lockonDuration = 0f;

    public Transform transform;
    public GameObject bulletPrefab;
    public GameObject attackObjectPrefab;
    public GameObject windupParticlePrefab;
    public GameObject attackParticlePrefab;
    public float particleOriginOffset = 0.0f;
    public Tower originReference; // I am very open to a better way of doing this so please if you can rearchitect it go ahead. 
    public Animator animator;

    #region TAGS
    [Header("Spray Tag")]
    public bool spray = false;
    public int sprayDamage = 1;
    public float additionalSprayRange = 2;

    [Header("Strikethrough Tag")]
    public bool strikethrough = true;
    public int strikethroughDamage = 1;
    public int strikethroughReach = 10;
    public int strikethroughBeamWidth = 4;
    public Matrix4x4 strikethroughMatrix;
    #endregion

    protected List<Target> targetsToShoot = new();
    protected bool attacking = false;

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
        }
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
        if (animator == null) return;

        animator.SetTrigger("Attack");
    }
}