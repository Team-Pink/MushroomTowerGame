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
        if (cooldownTimer < attackCooldown)
        {
            cooldownTimer += Time.deltaTime * attackCooldown;
            return;
        }

        //Allow animation to play here or in another location through prompt
        
        if (delayTimer < attackDelay)
        {
            delayTimer += Time.deltaTime * attackDelay;
            return;
        }
    }
}
