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
            Bullet bulletRef;

            bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, transform.position + Vector3.up * 2, Quaternion.identity).GetComponent<Bullet>();

            bulletRef.timeToTarget = attackDelay;
            bulletRef.target = target;
        }
    }
    public void AnimateAttack()
    {
        if (animator == null) return;

        animator.SetTrigger("Attack");
    }
}