using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #region TAG SPECIFIC
    public int tagSpecificDamage;
    public HashSet<Enemy> tagSpecificEnemiesHit = new HashSet<Enemy>(); //enemies that were hit as a result of tags like spray
    #endregion

    // private Animator

    public IEnumerator CommenceAttack(float animationDelay = 0.0f)
    {
        yield return new WaitForSeconds(delayToTarget + animationDelay); //this was originally a timer in the update loop but if you want coroutine's then sure I'll see what I can do.
        // play impact animation
        if (hitParticlePrefab != null) Instantiate(hitParticlePrefab, target.enemy.transform.position, Quaternion.identity);
        if (hitSoundEffect != null) AudioManager.PlaySoundEffect(hitSoundEffect.name, 1);

        Attacker attackerComponent = originTower.AttackerComponent;

        if (attackerComponent is SingleAttacker)
        {
            target.enemy.TakeDamage(damage);

            ///Strikethrough Tag
            if (attackerComponent.strikethrough)
            {
                if (tagSpecificEnemiesHit.Count < 1)
                    Debug.LogError("No Enemies Detected Please Resolve");

                foreach (var enemy in tagSpecificEnemiesHit)
                {
                    enemy.TakeDamage(tagSpecificDamage);
                    
                    HandleNonTargetEnemyDeath(enemy);
                }
                
            }
        }

        if (attackerComponent is AreaAttacker)
        {
            // get everything hit by the attack
            foreach (Enemy enemyHit in areaHitTargets)
            {
                enemyHit.TakeDamage(damage);
                if (enemyHit.CheckIfDead())
                {
                    HandleNonTargetEnemyDeath(enemyHit);
                }
            }

            ///Spray Tag
            if (attackerComponent.spray)
            {
                if (tagSpecificEnemiesHit is null)
                    Debug.LogError("No Enemies Detected Please Resolve");

                foreach (Enemy enemyHit in tagSpecificEnemiesHit)
                {
                    enemyHit.TakeDamage(tagSpecificDamage);
                    
                    HandleNonTargetEnemyDeath(enemyHit);
                }
            }
        }

        HandleTargetEnemyDeath();

        Destroy(gameObject); // Destroy this object
    }

    /// <summary>
    /// Checks if the target enemy has died and if so perform nessesary actions
    /// </summary>
    void HandleTargetEnemyDeath()
    {
        if (target.enemy.CheckIfDead())
        {
            // extract exp
            originTower.storedExperience += target.enemy.expValue;
            target.enemy.expValue = 0;

            if (originTower.GetAccelerate()) // Accelerate logic
            {
                if (!target.enemy.Dead) // The bool Dead is set in OnDeath() so if it is false we can be sure this attack dealt the killing blow as the enemy has no health but hasn't "died" yet.
                {
                    originTower.accelerated = true; // this could be called from elsewhere if neccesary
                    originTower.accelTimer = 0;
                    originTower.AttackerComponent.attackDelay *= originTower.accelSpeedMod;// modify attack delay
                }
            }
            target.enemy.OnDeath(); // enemy on death
        }
    }

    /// <summary>
    /// Checks if a non target enemy has died and if so perform nessesary actions
    /// </summary>
    /// <param name="enemy"></param>
    void HandleNonTargetEnemyDeath(Enemy enemy)
    {
        if (enemy == target.enemy)
            return;
        
        if (enemy.CheckIfDead())
        {
            // extract exp
            originTower.storedExperience += enemy.expValue;
            enemy.expValue = 0;
            enemy.OnDeath(); // enemy on death
        }
    }
}

//Rundown of current functionality
// runs a coroutine that waits to delay damage until the attack has reached the target
// when it does reach the target
// deal damage to the given targets
// if the target dies modify exp and accelerate and then call the enemy's OnDeath()
// check if originTower uses an area attack
// if yes get the damaged AffectedEnemies from the AttackerComponent and do death checks on them
// finally destroy this.
