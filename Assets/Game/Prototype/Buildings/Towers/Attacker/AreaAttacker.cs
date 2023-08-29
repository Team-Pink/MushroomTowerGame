using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AreaAttacker : Attacker
{
    float damageRadius = 3f;

    public override void Attack(HashSet<Target> targets)
    {
        //Play attack animation here

        if (!CheckDelayTimer()) return;

        if (cooldownTimer == 0f)
        {
            LayerMask mask = LayerMask.GetMask("Enemy");

            Debug.Log("Area Attack");

            foreach (var target in targets)
            {
                foreach (var collision in Physics.OverlapSphere(target.position, damageRadius, mask))
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    if (enemy is null)
                        continue;
                    enemy.TakeDamage(damage);
                }
            }
        }

        if (!CheckCooldownTimer()) return;

        cooldownTimer = 0f;
        delayTimer = 0f;
    }
}
