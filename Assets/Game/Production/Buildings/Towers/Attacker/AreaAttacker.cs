using System.Collections.Generic;
using UnityEngine;

public class AreaAttacker : Attacker
{
    float damageRadius = 3f;
    public HashSet<Enemy> affectedEnemies = new HashSet<Enemy>();

    public override void Attack(HashSet<Target> targets) //  I need a way to get references to the things hit by the aoe out.
    {
        //Play attack animation here

        if (!CheckDelayTimer()) return;

        if (cooldownTimer == 0f)
        {
            affectedEnemies.Clear();
            LayerMask mask = LayerMask.GetMask("Enemy");

            Debug.Log("Area Attack");

            foreach (var target in targets)
            {
                AttackObject areaAttack = GenerateAttackObject(target);
                areaAttack.StartCoroutine(areaAttack.CommenceAttack());
                foreach (var collision in Physics.OverlapSphere(target.position, damageRadius, mask))
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    if (enemy is null)
                        continue;
                    //enemy.StartCoroutine(target.enemy.TakeDamage(damage)); //create an attack object instead                  
                    affectedEnemies.Add(enemy); // grabs references to all hit enemies which really should be done by a targeter.
                }
                areaAttack.areaHitTargets = affectedEnemies;
                AnimateAttack(target); 
            }
        }

        if (!CheckCooldownTimer()) return;

        cooldownTimer = 0f;
        delayTimer = 0f;
    }
}
