using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    public GameObject attackObjectPrefab;
    public Tower originReference; // I am very open to a better way of doing this so please if you can rearchitect it go ahead. 
    public Animator animator;

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