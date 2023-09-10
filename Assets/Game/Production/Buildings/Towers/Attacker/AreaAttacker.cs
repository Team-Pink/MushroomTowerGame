using System.Collections.Generic;
using UnityEngine;

public class AreaAttacker : Attacker
{
    float damageRadius = 3f;

    public override void Attack(HashSet<Target> targets)
    {
        if (cooldownTimer == 0.0f)
        {
            AnimateAttack();

            if (!CheckDelayTimer()) return;

            LayerMask mask = LayerMask.GetMask("Enemy");

            Debug.Log("Area Attack");

            foreach (var target in targets)
            {
                foreach (var collision in Physics.OverlapSphere(target.position, damageRadius, mask))
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    if (enemy is null)
                        continue;
                    target.enemy.StartCoroutine(target.enemy.TakeDamage(damage, attackDelay));
                }
                AnimateProjectile(target);
            }
        }

        if (!CheckCooldownTimer()) return;

        cooldownTimer = 0f;
        delayTimer = 0f;
    }
}
