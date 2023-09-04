using System.Collections.Generic;
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
                Collider[] mainCollisions = Physics.OverlapSphere(target.position, damageRadius, mask);
                foreach (var collision in mainCollisions)
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    if (enemy is null)
                        continue;
                    enemy.StartCoroutine(enemy.TakeDamage(damage, attackDelay));
                }

                #region Tag Applications
                if (spray)
                { 
                    foreach (var sprayCollision in Physics.OverlapSphere(target.position, damageRadius + additionalSprayRange, mask))
                    {
                        bool isMainCollision = false;
                        Enemy enemy = sprayCollision.gameObject.GetComponent<Enemy>();

                        foreach (var mainCollision in mainCollisions)
                        {
                            if (mainCollision == sprayCollision)
                                isMainCollision = true;
                        }

                        if (!isMainCollision)
                            enemy.StartCoroutine(enemy.TakeDamage(sprayDamage, attackDelay));
                    }
                }


                #endregion

                AnimateAttack(target);
            }
        }

        if (!CheckCooldownTimer()) return;

        cooldownTimer = 0f;
        delayTimer = 0f;
    }
}
