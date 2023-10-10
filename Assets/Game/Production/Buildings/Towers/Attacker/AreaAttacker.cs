using System.Collections.Generic;
using UnityEngine;

public class AreaAttacker : Attacker
{
    public float damageRadius = 3f;

    public override void Attack(HashSet<Target> targets) //  I need a way to get references to the things hit by the aoe out.
    {

        if (!attacking)
        {
            AnimateAttack();

            if (windupParticlePrefab != null)
            {
                GameObject particle = Object.Instantiate(windupParticlePrefab, transform);
                particle.transform.position += new Vector3(0, particleOriginOffset, 0);
                Object.Destroy(particle, animationLeadIn);
            }

            attacking = true;

            LayerMask mask = LayerMask.GetMask("Enemy");




            foreach (var target in targets)
            {


                AttackObject areaAttack = GenerateAttackObject(target);

                areaAttack.hitParticlePrefab = hitParticlePrefab;
                areaAttack.hitSoundEffect = attackHitSoundEffect;

                if(lobProjectile)
                {
                    areaAttack.noTracking = true;
                }


                areaAttack.areaHitTargets = new(); 
                areaAttack.damageRadius = damageRadius;
                areaAttack.mask = mask;

                #region Tag Applications 
                if (spray)
                {
                    Collider[] mainCollisions = Physics.OverlapSphere(target.position, damageRadius, mask);
                    areaAttack.tagSpecificDamage = sprayDamage;
                    areaAttack.tagSpecificEnemiesHit = Spray(target, mainCollisions, mask);
                }
                #endregion

                areaAttack.StartCoroutine(areaAttack.CommenceAttack(animationLeadIn));
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
