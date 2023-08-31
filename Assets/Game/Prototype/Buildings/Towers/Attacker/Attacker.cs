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
        GameObject bulletRef;

        bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, transform.position + Vector3.up * 2, Quaternion.identity);

        bulletRef.GetComponent<Bullet>().timeToTarget = attackDelay;
        bulletRef.GetComponent<Bullet>().target = target;
    }
}