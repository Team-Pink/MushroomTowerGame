using System.Collections.Generic;
using UnityEngine;

public class AreaAttacker : Attacker
{
    public float damageRadius = 3f;
    public HashSet<Enemy> affectedEnemies = new HashSet<Enemy>();

    public override void Attack(HashSet<Target> targets) //  I need a way to get references to the things hit by the aoe out.
    {
        if (!attacking)
        {
            affectedEnemies.Clear();
            AnimateAttack();
            attacking = true;

            LayerMask mask = LayerMask.GetMask("Enemy");

            Debug.Log("Area Attack");

            foreach (var target in targets)
            {
                AttackObject areaAttack = GenerateAttackObject(target);
                areaAttack.StartCoroutine(areaAttack.CommenceAttack());

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
                    HashSet<Enemy> sprayTargets = new HashSet<Enemy>();
                    areaAttack.tagSpecificDamage = sprayDamage;

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
                        {
                            sprayTargets.Add(enemy);
                        }
                            
                    }
                    areaAttack.tagSpecificEnemiesHit = sprayTargets;
                }
                #endregion

                targetsToShoot.Add(target);
            }
        }

        if (!CheckCooldownTimer()) return;

        cooldownTimer = 0f;
        delayTimer = 0f;
        attacking = false;
        targetsToShoot.Clear();
    }
}
