using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Attacking : MonoBehaviour
{
    #region Public Functions
    /// <summary>
    /// Uses Distance equation to determine if the tower is within the enemy's radius
    /// </summary>
    /// <param name="tower"></param>
    /// <param name="enemy"></param>
    /// <param name="attackRange"></param>
    /// <returns></returns>
    public bool DistanceCheck(GameObject tower, GameObject enemy, float attackRange)
    {
        Vector3 towerPos = tower.transform.position;
        Vector3 enemyPos = enemy.transform.position;

        float distance = Mathf.Sqrt(Mathf.Pow(enemyPos.x - towerPos.x, 2) + Mathf.Pow(enemyPos.z - towerPos.z, 2));

        if (distance <= attackRange)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// call the enemy to attack the heart
    /// </summary>
    /// <param name="tower"></param>
    /// <param name="enemy"></param>
    public void BasicEnemyDamageHeart(TempTower tower, TempEnemy enemy)
    {
        if (enemy.isAttacking == false)
        {
            Debug.Log("Enemy can attack");
            enemy.isAttacking = true;

            CallAttackLogic(tower, enemy);
        }
    }
    #endregion

    #region Private Functions
    void CallAttackLogic(TempTower tower, TempEnemy enemy)
    {
        StartCoroutine(Attack(tower, enemy));
    }

    IEnumerator Attack(TempTower tower, TempEnemy enemy)
    {
        Debug.Log("Tower health before: " + tower.health);

        tower.health -= enemy.attackDamage;

        Debug.Log("Tower health after: " + tower.health);

        yield return new WaitForSeconds(enemy.timeBetweenAttacks);

        enemy.isAttacking = false;

        yield return null;
    }
    #endregion
}
