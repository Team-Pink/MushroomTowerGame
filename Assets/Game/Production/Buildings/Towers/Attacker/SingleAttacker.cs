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
                    #region TAG IMPLEMENTATION
                    if (strikethrough)
                    {
                        LayerMask mask = LayerMask.GetMask("Enemy");

                        Vector3 direction = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z).normalized;
                        float medianReach = (strikethroughReach / 2);

                        Quaternion rotation = Quaternion.LookRotation(direction);
                        Vector3 centerPosition = transform.position + direction * medianReach;
                        Vector3 scale = new Vector3(strikethroughBeamWidth / 2, 1000, medianReach / 2);

                        Collider[] collisions = Physics.OverlapBox(centerPosition, scale, rotation, mask);

                        foreach (Collider collision in collisions)
                        {
                            Enemy enemy = collision.GetComponent<Enemy>();
                            enemy.StartCoroutine(enemy.TakeDamage(strikethroughDamage, attackDelay));
                        }

                    }
                    #endregion

                    target.enemy.StartCoroutine(target.enemy.TakeDamage(damage, attackDelay));

                    AnimateAttack(target);
                }
        }

        if (!CheckCooldownTimer()) return;

        delayTimer = 0;
        cooldownTimer = 0;
    }
}
