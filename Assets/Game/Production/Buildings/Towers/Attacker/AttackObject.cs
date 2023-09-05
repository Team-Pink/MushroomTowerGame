using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackObject : MonoBehaviour
{
    public float delayToTarget; // time until the attack reaches the target
    public HashSet<Target> targetEnemy; // targets of the attack
    public Tower originTower; // origin of the attack

    // private Animator


    // Update is called once per frame
    void Update()
    {
        if (delayToTarget > 0) delayToTarget -= Time.deltaTime; // check time to target only attack after the delay time is complete.
        else
        {




            foreach (Target target in targetEnemy)
            {
                // play impact animation

                originTower.AttackerComponent.Attack(targetEnemy);// Do attack on target

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
                }

                if (originTower.AttackerComponent is AreaAttacker)
                {
                    // get everything hit by the attack
                    foreach (Enemy EnemyHit in (originTower.AttackerComponent as AreaAttacker).affectedEnemies)
                    {
                        if (target.enemy.CheckIfDead())
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
}

//Rundown of current functionality
// runs a clock that works off of the attack delay to delay damage until the attack has reached the target  // Could be replaced with a coroutine that waits.
// when it does reach the target
// deal damage to the given targets
// if the target dies modify exp and accelerate and then call the enemy's OnDeath()
// check if originTower uses an area attack
// if yes get the damaged AffectedEnemies from the AttackerComponent and do death checks on them
// finally destroy this.
