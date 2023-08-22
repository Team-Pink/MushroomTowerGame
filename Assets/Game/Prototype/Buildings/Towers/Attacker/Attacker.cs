using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker
{
    public int damage;

    public float attackCooldown;
    protected float cooldownTimer = 1f;

    public float attackDelay;
    protected float delayTimer = 0f;

    public virtual void Attack(HashSet<Target> targets)
    {
        Debug.LogWarning("Use one of the other methods of attacking");
    }
    protected bool CheckCooldownTimer()
    {
        if (cooldownTimer < attackCooldown)
        {
            cooldownTimer += Time.deltaTime * attackCooldown;
            return false;
        }

        return true;
    }
    protected bool CheckDelayTimer()
    {
        if (delayTimer < attackDelay)
        {
            delayTimer += Time.deltaTime * attackDelay;
            return false;
        }

        return true;
    }
}
