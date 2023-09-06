using System.Collections.Generic;
using UnityEngine;

public class SingleAttacker : Attacker
{
    public override void Attack(HashSet<Target> targets)
    {
        //Play attack animation here

        if (!CheckDelayTimer()) return;

        if (cooldownTimer == 0f)
        {
            Debug.Log("Single Attack");
            foreach (var target in targets)
                if (target.enemy is not null)
                {
                    //target.enemy.StartCoroutine(target.enemy.TakeDamage(damage)); // swap this with generate attack object
                    AttackObject singleAttack =GenerateAttackObject(target);
                    singleAttack.StartCoroutine(singleAttack.CommenceAttack());
                    AnimateAttack(target);
                }
        }

        if (!CheckCooldownTimer()) return;

        delayTimer = 0;
        cooldownTimer = 0;
    }
}
