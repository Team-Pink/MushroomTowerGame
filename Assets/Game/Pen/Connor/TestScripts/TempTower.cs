using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTower : MonoBehaviour
{
    public float health = 5f;

    List<TempEnemy> enemyList = new();

    Attacking attacking;

    public bool isAlive;

    private void Awake()
    {
        
        attacking = gameObject.AddComponent<Attacking>();
    }

    private void Update()
    {
        if(health <= 0)
        {
            gameObject.SetActive(false);
        }

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
        if(other.GetComponent<TempEnemy>() != null)
        {
            TempEnemy enemy = other.GetComponent<TempEnemy>();
            enemyList.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<TempEnemy>() != null)
        {
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
