using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaAttacker : Attacker
{
    float damageRadius;

    public override void Attack(HashSet<Target> targets)
    { 
        base.Attack(targets);

        if (delayTimer < attackDelay && cooldownTimer < attackCooldown) return;

        LayerMask mask = LayerMask.GetMask("Enemy");

        foreach(var target in targets)
        {
            foreach (var collision in Physics.OverlapSphere(target.position, damageRadius, mask))
            {
                collision.GetComponent<Enemy>().health -= damage;
            }
        }

        cooldownTimer = 0f;
        delayTimer = 0f;
    }
}
