using System.Collections.Generic;
using UnityEngine;

public class SingleAttacker : Attacker
{
    public override void Attack(HashSet<Target> targets)
    {
        if (!attacking)
        {
            AnimateAttack();
            attacking = true;

            Debug.Log("Single Attack");
            foreach (var target in targets)
            {
                if (target.enemy is not null)
                {
                    //target.enemy.StartCoroutine(target.enemy.TakeDamage(damage)); // swap this with generate attack object
                    AttackObject singleAttack =GenerateAttackObject(target);
                    singleAttack.StartCoroutine(singleAttack.CommenceAttack());

                    targetsToShoot.Add(target);
                }
            }
        }

        if (!CheckCooldownTimer()) return;

        delayTimer = 0;
        cooldownTimer = 0;
        attacking = false;
        targetsToShoot.Clear();
    }
}
