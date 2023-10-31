using System.Collections;
using System.Collections.Generic;
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
    public Tower originTower; // origin of the attack
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
    bool returningToTower = false;
    float returnToTowerTime = 0;
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

        Attacker attackerComponent = originTower.AttackerComponent;

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

        if (originTower.AttackerComponent.bounce && returningToTower)
        {
            yield return new WaitForSeconds(returnToTowerTime);
            originTower.AttackerComponent.bounceBulletInTowerPossession = true;
            attackerComponent.returning = returningToTower;
        }//this is for bounce only to allow the tower to shoot again.

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
        if (_velocity <= 0)
        {
            _velocity = GenericUtility.CalculateVelocity(GenericUtility.CalculateDistance(originTower.transform.position, target.position), delayToTarget);
        }

        if (target.enemy == null)
        {
            Debug.Log("TARGET LOST RETURNING TO TOWER");
            BounceToNextTarget(true, null);
        }

        hitList.Add(target.enemy);

        int hitCount = hitList.Count;
        LayerMask mask = LayerMask.GetMask("Enemy");

        if (hitCount >= attackerComponent.bounceHitLimit)
        {
            returningToTower = true;
            float timeToTower = GenericUtility.CalculateTime(_velocity, GenericUtility.CalculateDistance(target.enemy.transform.position, originTower.transform.position));
            originTower.AttackerComponent.AnimateBounceProjectileToTower(target, timeToTower);
            returnToTowerTime = timeToTower;
            return;
        }

        Enemy newTarget = null;
        Collider[] potentialTargets = Physics.OverlapSphere(originTower.transform.position, originTower.TargeterComponent.range, mask);

        foreach (var potentialTarget in potentialTargets)
        {
            Enemy enemy = potentialTarget.gameObject.GetComponent<Enemy>();
            if (hitList.Contains(enemy) || enemy.CheckIfDead()) continue;

            if (newTarget == null) newTarget = enemy;
            else
            {
                float distanceFromCurrentTargetA = GenericUtility.CalculateDistance(target.enemy.transform.position, newTarget.transform.position);
                float distanceFromCurrentTargetB = GenericUtility.CalculateDistance(target.enemy.transform.position, enemy.transform.position);

                if (distanceFromCurrentTargetA > distanceFromCurrentTargetB) newTarget = enemy;
            }
        }

        if (newTarget == null)
        {
            BounceToNextTarget(true, null);
            return;
        }
        else
        {
            BounceToNextTarget(false, newTarget);
        }
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
        AttackObject attackInProgress = MonoBehaviour.Instantiate(originTower.GetAttackObjectPrefab()).GetComponent<AttackObject>();
        attackInProgress.damage = damage;
        attackInProgress.delayToTarget = timeToNextTarget;
        attackInProgress.originTower = originTower;
        attackInProgress.target = enemy;
        attackInProgress.hitList = hitList;
        attackInProgress.returningToTower = returningToTower;
        attackInProgress._velocity = _velocity;
        return attackInProgress;
    }

    void BounceToNextTarget(bool bounceToTower, Enemy newTarget)
    {
        if (bounceToTower)
        {
            returningToTower = true;
            float timeToTower = GenericUtility.CalculateTime(_velocity, GenericUtility.CalculateDistance(transform.position, originTower.transform.position));
            originTower.AttackerComponent.AnimateBounceProjectileToTower(target, timeToTower);
            returnToTowerTime = timeToTower;
            return;
        }
        else
        {
            float timeToTarget = GenericUtility.CalculateTime(_velocity, GenericUtility.CalculateDistance(transform.position, newTarget.transform.position));
            Target newTargetEnemy = new Target();
            newTargetEnemy.enemy = newTarget;
            AttackObject newAttackObject = GenerateBounceAttackObject(newTargetEnemy, timeToTarget);
            newAttackObject.StartCoroutine(newAttackObject.CommenceAttack());
            originTower.AttackerComponent.AnimateBounceProjectileToEnemy(target, newTargetEnemy, timeToTarget);
        }
    }
}