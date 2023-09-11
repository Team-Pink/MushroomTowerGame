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

    // private Animator

    public IEnumerator CommenceAttack()
    {
        yield return new WaitForSeconds(delayToTarget); //this was originally a timer in the update loop but if you want coroutine's then sure I'll see what I can do.
        
            // play impact animation

            //originTower.AttackerComponent.Attack(targetEnemy);// Do attack on target
            target.enemy.TakeDamage(damage);

            if (target.enemy.CheckIfDead())
            {
                // extract exp
                originTower.storedExperience += target.enemy.expValue;
                target.enemy.expValue = 0;

                if (originTower.GetAccelerate()) // Accelerate logic
                {
                    if (!target.enemy.Dead)
                    {
                        originTower.accelerated = true; // this could be called from elsewhere if neccesary
                        originTower.accelTimer = 0;
                        originTower.AttackerComponent.attackDelay *= originTower.accelSpeedMod;// modify attack delay
                    }
                    else
                    {
                        originTower.AttackerComponent.attackDelay *= originTower.decreaseAccel;// modify attack delay
                    }
                }
                target.enemy.OnDeath(); // enemy on death               
           

            if (originTower.AttackerComponent is AreaAttacker)
            {
                // get everything hit by the attack
                foreach (Enemy enemyHit in areaHitTargets)
                {
                    enemyHit.TakeDamage(damage);
                    if (enemyHit.CheckIfDead())
                    {
                        // extract exp
                        originTower.storedExperience += target.enemy.expValue;
                        target.enemy.expValue = 0;
                        target.enemy.OnDeath(); // enemy on death               
                    }
                }
            }
        }
        Destroy(gameObject);
        // Destroy this

    }
}

//Rundown of current functionality
// runs a clock that works off of the attack delay to delay damage until the attack has reached the target  // Could be replaced with a coroutine that waits.
// when it does reach the target
// deal damage to the given targets
// if the target dies modify exp and accelerate and then call the enemy's OnDeath()
// check if originTower uses an area attack
// if yes get the damaged AffectedEnemies from the AttackerComponent and do death checks on them
// finally destroy this.
