using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An object responsible for carrying out the effects of an attack on an enemy from the time after an attack is launched to when it resolves.
/// </summary>
public class AttackObject : MonoBehaviour
{
    public int damage; // damage dealt to target
    public float delayToTarget; // time until the attack reaches the target
    public Target target; // targets of the attack
    public Shroom originShroom; // origin of the attack
    public HashSet<Enemy> areaHitTargets;
    public GameObject hitParticlePrefab;
    public AudioClip hitSoundEffect;
    public float damageRadius;
    public LayerMask mask;
    public bool noTracking = false;

    #region TAG SPECIFIC VARIABLES
    public int tagSpecificDamage;
    public HashSet<Enemy> tagSpecificEnemiesHit = new HashSet<Enemy>(); //enemies that were hit as a result of tags like spray

    #region BOUNCE
    //Bounce Tag
    List<Enemy> hitList = new List<Enemy>();
    Vector3 lastHitPosition;
    bool returningToShroom = false;
    float returnToShroomTime = 0;
    float _velocity = 0;
    #endregion
    #endregion

    // private Animator

    public IEnumerator CommenceAttack(float animationDelay = 0.0f)
    {
        if (noTracking)
        {
            Vector3 NoTrackingPos = new Vector3() + target.position;
            target.position = NoTrackingPos;
            Debug.DrawLine(target.position + Vector3.up * 5, target.position, Color.blue, 10);

        }

        yield return new WaitForSeconds(delayToTarget + animationDelay); //this was originally a timer in the update loop but if you want coroutine's then sure I'll see what I can do.

        // play impact animation
        if (target.enemy != null)
        {
            if (hitParticlePrefab != null) Instantiate(hitParticlePrefab, target.position, Quaternion.identity);
            if (hitSoundEffect != null) AudioManager.PlaySoundEffect(hitSoundEffect.name, 1);
        }

        Attacker attackerComponent = originShroom.AttackerComponent;

        if (attackerComponent is SingleAttacker)
        {

            target.enemy.TakeDamage(damage);

            ///Strikethrough Tag
            if (attackerComponent.strikethrough)
            {
                Stikethrough();
            }

            if (attackerComponent.bounce)
            {
                Bounce(attackerComponent);
            }
        }
        if (attackerComponent is AreaAttacker)
        {
            // get everything in the area of the attack
            Collider[] mainCollisions = Physics.OverlapSphere(target.position, damageRadius, mask);
            foreach (var collision in mainCollisions)
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy is null) continue;
                areaHitTargets.Add(enemy); // grabs references to all hit enemies which really should be done by the attack object.
            }

            // get everything hit by the attack
            foreach (Enemy enemyHit in areaHitTargets)
            {
                enemyHit.TakeDamage(damage);
                if (enemyHit.CheckIfDead()) HandleNonTargetEnemyDeath(enemyHit);
            }

            ///Spray Tag
            if (attackerComponent.spray)
            {
                Spray();
            }
        }

        HandleTargetEnemyDeath();

        if (originShroom.AttackerComponent.bounce && returningToShroom) 
        {
            yield return new WaitForSeconds(returnToShroomTime);
            originShroom.AttackerComponent.bounceBulletInShroomPossession = true;
            attackerComponent.returning = returningToShroom;
        }//this is for bounce only to allow the shroom to shoot again.

        Destroy(gameObject); // Destroy this object
    }

    /// <summary>
    /// Checks if the target enemy has died and if so perform nessesary actions
    /// </summary>
    void HandleTargetEnemyDeath()
    {
        if (target.enemy.CheckIfDead())
        {
            target.enemy.OnDeath(); // enemy on death
        }
    }

    /// <summary>
    /// Checks if a non target enemy has died and if so perform nessesary actions
    /// </summary>
    /// <param name="enemy">
    /// the enemy your reffering to
    /// </param>
    void HandleNonTargetEnemyDeath(Enemy enemy)
    {
        if (enemy == target.enemy)
            return;

        if (enemy.CheckIfDead())
        {
            enemy.OnDeath(); // enemy on death
        }
    }

    void Stikethrough()
    {
        if (tagSpecificEnemiesHit.Count < 1)
            Debug.LogError("No Enemies Detected Please Resolve");

        foreach (var enemy in tagSpecificEnemiesHit)
        {
            enemy.TakeDamage(tagSpecificDamage);

            HandleNonTargetEnemyDeath(enemy);
        }
    }

    void Bounce(Attacker attackerComponent)
    {
        if (_velocity <= 0) _velocity = originShroom.GetProjectileSpeed();

        if (!target.enemy)
        {
            Debug.Log("TARGET LOST RETURNING TO SHROOM");
            BounceToNextTarget(true, null);
            return;
        }

        lastHitPosition = target.enemy.transform.position;

        hitList.Add(target.enemy);

        int hitCount = hitList.Count;
        LayerMask mask = LayerMask.GetMask("Enemy");

        if (hitCount >= attackerComponent.bounceHitLimit)
        {
            BounceToNextTarget(true, null);
            return;
        }

        Enemy newTarget = null;
        Collider[] potentialTargets = Physics.OverlapSphere(originShroom.transform.position, originShroom.TargeterComponent.range, mask);

        foreach (var potentialTarget in potentialTargets)
        {
            Enemy enemy = potentialTarget.gameObject.GetComponent<Enemy>();
            if (hitList.Contains(enemy) || enemy.CheckIfDead()) continue;

            if (newTarget == null) newTarget = enemy;
            else
            {
                float distanceFromCurrentTargetA = Vector3.Distance(lastHitPosition, newTarget.transform.position);
                float distanceFromCurrentTargetB = Vector3.Distance(lastHitPosition, enemy.transform.position);

                if (distanceFromCurrentTargetA > distanceFromCurrentTargetB) newTarget = enemy;
            }
        }

        if (newTarget == null) BounceToNextTarget(true, null);
        else BounceToNextTarget(false, newTarget);
    }

    void Spray()
    {
        if (tagSpecificEnemiesHit is null)
            Debug.LogError("No Enemies Detected Please Resolve");

        foreach (Enemy enemyHit in tagSpecificEnemiesHit)
        {
            enemyHit.TakeDamage(tagSpecificDamage);

            HandleNonTargetEnemyDeath(enemyHit);
        }
    }

    AttackObject GenerateBounceAttackObject(Target enemy, float timeToNextTarget)
    {
        AttackObject attackInProgress = MonoBehaviour.Instantiate(originShroom.GetAttackObjectPrefab()).GetComponent<AttackObject>();
        attackInProgress.damage = damage;
        attackInProgress.delayToTarget = timeToNextTarget;
        attackInProgress.originShroom = originShroom;
        attackInProgress.target = enemy;
        attackInProgress.hitList = hitList;
        attackInProgress.lastHitPosition = hitList.Last().transform.position;
        attackInProgress.returningToShroom = returningToShroom;
        attackInProgress._velocity = _velocity;

        return attackInProgress;
    }

    void BounceToNextTarget(bool bounceToShroom, Enemy newTarget)
    {
        if (bounceToShroom)
        {
            returningToShroom = true;
            float timeToShroom = Vector3.Distance(lastHitPosition, originShroom.transform.position) / _velocity;
            originShroom.AttackerComponent.AnimateBounceProjectileToShroom(target, timeToShroom);
            returnToShroomTime = timeToShroom;
        }
        else
        {
            float distance = Vector3.Distance(lastHitPosition, newTarget.transform.position);

            float timeToTarget = distance / _velocity;
            Target newTargetEnemy = new Target();
            newTargetEnemy.enemy = newTarget;
            AttackObject newAttackObject = GenerateBounceAttackObject(newTargetEnemy, timeToTarget);
            newAttackObject.StartCoroutine(newAttackObject.CommenceAttack());
            originShroom.AttackerComponent.AnimateBounceProjectileToEnemy(target, newTargetEnemy, timeToTarget);
        }
    }
}
