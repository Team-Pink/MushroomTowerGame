using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TempTower : MonoBehaviour
{
    public float health = 5f;

    public List<TempEnemy> enemyList = new();

    Attacking attacking;

    public bool isAlive = true;

    public bool Pylon = false;
    public bool Hub = false;

    private void Awake()
    {
        
        attacking = gameObject.AddComponent<Attacking>();
    }

    private void Update()
    {
        if(health <= 0)
        {
            isAlive = false;
        }

        foreach(var enemy in enemyList)
        {
            if (enemy.target != null && enemy.Ranged == true && Pylon == true)
            {
                if(enemy.target.Pylon == false)
                    enemy.target = null;
            }

            if (attacking.DistanceCheck(gameObject, enemy.gameObject, enemy.attackRange) && (enemy.target == null || enemy.target == this) && isAlive == true)
            {                
                if(enemy.Ranged == true && Pylon == true)
                {
                    enemy.target = this;
                    attacking.BasicEnemyDamageHeart(this, enemy);
                }
                else if (Hub == true)
                {
                    enemy.target = this;
                    attacking.BasicEnemyDamageHeart(this, enemy);
                }
            }
        }

        if (!isAlive)
        {
            gameObject.SetActive(false);



            //Delete this eventually, used for testing purposes
            if (EditorApplication.isPlaying)
            {
                EditorApplication.Beep();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<TempEnemy>() != null)
        {
            TempEnemy enemy = other.GetComponent<TempEnemy>();
            if(!enemy.isDead)
            {
                enemyList.Add(enemy);
                enemy.detected.Add(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<TempEnemy>() != null)
        {
            TempEnemy enemy = other.GetComponent<TempEnemy>();
            if(enemyList.Contains(enemy))
            {
                if (enemy.target == this)
                    enemy.target = null;
                enemyList.Remove(enemy);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, GetComponent<CapsuleCollider>().radius);
    }
}
