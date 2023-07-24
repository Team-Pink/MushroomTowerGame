using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTower : MonoBehaviour
{
    public float health = 5f;

    List<TempEnemy> enemyList = new();

    Attacking attacking;

    private void Awake()
    {
        
        attacking = gameObject.AddComponent<Attacking>();
    }

    private void Update()
    {
        foreach(var enemy in enemyList)
        {
            if (attacking.DistanceCheck(gameObject, enemy.gameObject, enemy.attackRange))
            {
                attacking.BasicEnemyDamageHeart(this, enemy);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Detected collision");
        if(other.GetComponent<TempEnemy>() != null)
        {
            Debug.Log("collided with an enemy");
            TempEnemy enemy = other.GetComponent<TempEnemy>();
            enemyList.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Detected object leaving trigger");
        if (other.GetComponent<TempEnemy>() != null)
        {
            Debug.Log("Enemy has left tower range");
            TempEnemy enemy = other.GetComponent<TempEnemy>();
            enemyList.Remove(enemy);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, GetComponent<CapsuleCollider>().radius);
    }
}
