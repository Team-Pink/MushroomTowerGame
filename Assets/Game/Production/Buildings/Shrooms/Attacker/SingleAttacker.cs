using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SingleAttacker : Attacker
{
    public override void Attack(HashSet<Target> targets)
    {
        if (targets == null || targets.Count == 0) return;

        Target target = targets.First();

        if (!attacking && target.enemy is not null)
        {
            attacking = true;
            currentTarget = target.enemy.transform;

            AnimateAttack();

            //AttackObject singleAttack = GenerateAttackObject(target);
            //singleAttack.hitParticlePrefab = hitParticlePrefab;
            //singleAttack.hitSoundEffect = attackHitSoundEffect;

            if (bounce)
            {
                bounceBulletInShroomPossession = false;
            }


            if (windupParticlePrefab != null)
            {
                GameObject particle = Object.Instantiate(windupParticlePrefab, transform);
                particle.transform.position += new Vector3(0, particleOriginOffset, 0);
                Object.Destroy(particle, animationLeadIn);
            }

            //singleAttack.StartCoroutine(singleAttack.CommenceAttack(animationLeadIn));

            targetsToShoot.Add(target);
        }

        if (!CheckCooldownTimer()) return;

        delayTimer = 0;
        cooldownTimer = 0;
        attacking = false;
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
