using System.Collections.Generic;
using UnityEngine;

public class SingleAttacker : Attacker
{
    public override void Attack(HashSet<Target> targets)
    {
        if (!attacking)
        {
            AnimateAttack();

            if (windupParticlePrefab != null)
            {
                GameObject particle = Object.Instantiate(windupParticlePrefab, transform);
                particle.transform.position += new Vector3(0, particleOriginOffset, 0);
                Object.Destroy(particle, lockonDuration);
            }

            attacking = true;

            Debug.Log("Single Attack");
            foreach (var target in targets)
            {
                if (target.enemy is not null)
                {
                    AttackObject singleAttack = GenerateAttackObject(target);

                    #region TAG IMPLEMENTATION
                    if (strikethrough)
                    {
                        singleAttack.tagSpecificDamage = strikethroughDamage;

                        HashSet<Enemy> hitEnemies = Strikethrough(target);

                        singleAttack.tagSpecificEnemiesHit = hitEnemies;
                    }
                    #endregion

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

    HashSet<Enemy> Strikethrough(Target target)
    {
        LayerMask mask = LayerMask.GetMask("Enemy");
        Vector3 direction = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z).normalized;
        float medianReach = (strikethroughReach / 2);

        Quaternion rotation = Quaternion.LookRotation(direction);
        Vector3 centerPosition = transform.position + direction * medianReach;
        Vector3 scale = new Vector3(strikethroughBeamWidth / 2, 1000, medianReach);

        Collider[] collisions = Physics.OverlapBox(centerPosition, scale, rotation, mask);

        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

        foreach (Collider collision in collisions)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            Target targetEnemy;
            targetEnemy.enemy = enemy;
            hitEnemies.Add(enemy);
        }

        return hitEnemies;
    }
}
