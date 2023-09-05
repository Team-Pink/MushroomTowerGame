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

    public Transform transform;
    public GameObject bulletPrefab;
    public Animator animator;

    #region TAGS
    [Header("Spray Tag")]
    public bool spray = false;
    public int sprayDamage = 1;
    public float additionalSprayRange = 2;

    [Header("Strikethrough Tag")]
    public bool strikethrough;
    public int strikethroughDamage;
    public int strikethroughReach;
    public int strikethroughBeamWidth;
    #endregion

    public virtual void Attack(HashSet<Target> targets)
    {
        Debug.LogWarning("Use one of the other methods of attacking");
    }
    protected bool CheckCooldownTimer()
    {
        if (cooldownTimer < attackCooldown)
        {
            cooldownTimer += Time.deltaTime;
            return false;
        }
        return true;
    }
    protected bool CheckDelayTimer()
    {
        if (delayTimer < attackDelay)
        {
            delayTimer += Time.deltaTime;
            return false;
        }
        return true;
    }

    public void AnimateAttack(Target target)
    {
        Bullet bulletRef;

        bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, transform.position + Vector3.up * 2, Quaternion.identity).GetComponent<Bullet>();

        bulletRef.timeToTarget = attackDelay;
        bulletRef.target = target;

        if (animator != null)
            animator.SetTrigger("Attack");
    }
}