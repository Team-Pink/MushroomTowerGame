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
                    #region TAG IMPLEMENTATION
                    if (strikethrough)
                    {
                        LayerMask mask = LayerMask.GetMask("Enemy");

                        Vector3 direction = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z).normalized;
                        float medianReach = (strikethroughReach / 2);

                        Quaternion rotation = Quaternion.LookRotation(direction);
                        Vector3 centerPosition = transform.position + direction * medianReach;
                        Vector3 scale = new Vector3(strikethroughBeamWidth / 2, 1000, medianReach / 2); // test what is width vs length

                        Matrix4x4 matrix = Matrix4x4.TRS(centerPosition, rotation, scale);

                        Collider[] collisions = Physics.OverlapBox(centerPosition, scale, rotation, mask);

                        AttackObject StrikethroughHit = GenerateAttackObject(target);
                        StrikethroughHit.tagSpecificDamage = strikethroughDamage;

                        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

                        foreach (Collider collision in collisions)
                        {
                            Enemy enemy = collision.GetComponent<Enemy>();
                            Target targetEnemy;
                            targetEnemy.enemy = enemy;
                            hitEnemies.Add(enemy);
                        }
                        StrikethroughHit.tagSpecificEnemiesHit = hitEnemies;
                    }
                    #endregion

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
