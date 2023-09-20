using System.Collections.Generic;
using UnityEngine;

public class AreaAttacker : Attacker
{
    public float damageRadius = 3f;
    public HashSet<Enemy> affectedEnemies = new HashSet<Enemy>();

    public override void Attack(HashSet<Target> targets) //  I need a way to get references to the things hit by the aoe out.
    {
        spray = true;
        additionalSprayRange = 2f;
        sprayDamage = 1;

        if (!attacking)
        {
            affectedEnemies.Clear();
            AnimateAttack();

            if (windupParticlePrefab != null)
            {
                GameObject particle = Object.Instantiate(windupParticlePrefab, transform);
                particle.transform.position += new Vector3(0, particleOriginOffset, 0);
                Object.Destroy(particle, lockonDuration);
            }

            attacking = true;

            LayerMask mask = LayerMask.GetMask("Enemy");

            Debug.Log("Area Attack");

            foreach (var target in targets)
            {
                AttackObject areaAttack = GenerateAttackObject(target); 

                Collider[] mainCollisions = Physics.OverlapSphere(target.position, damageRadius, mask);
                foreach (var collision in mainCollisions)
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    if (enemy is null) continue;
                    affectedEnemies.Add(enemy); // grabs references to all hit enemies which really should be done by a targeter.
                }
                areaAttack.areaHitTargets = affectedEnemies;

                #region Tag Applications
                if (spray)
                {
                    areaAttack.tagSpecificDamage = sprayDamage;
                    areaAttack.tagSpecificEnemiesHit = Spray(target, mainCollisions, mask);
                }
                #endregion

                areaAttack.StartCoroutine(areaAttack.CommenceAttack());
                targetsToShoot.Add(target);
            }
        }

        if (!CheckCooldownTimer()) return;

        cooldownTimer = 0f;
        delayTimer = 0f;
        attacking = false;
        targetsToShoot.Clear();
    }

    HashSet<Enemy> Spray(Target target, Collider[] mainCollisions, LayerMask layerMask)
    {
        HashSet<Enemy> sprayTargets = new HashSet<Enemy>();

        foreach (var sprayCollision in Physics.OverlapSphere(target.position, damageRadius + additionalSprayRange, layerMask))
        {
            bool isMainCollision = false;
            Enemy enemy = sprayCollision.gameObject.GetComponent<Enemy>();

            foreach (var mainCollision in mainCollisions)
            {
                if (mainCollision == sprayCollision)
                {
                    isMainCollision = true;
                    break;
                }
            }

            if (!isMainCollision)
            {
                sprayTargets.Add(enemy);
               
            }

        }
        return sprayTargets;
    }
}
