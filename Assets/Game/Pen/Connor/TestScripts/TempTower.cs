using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            Debug.Log(gameObject + " Deado");
        }

        foreach(var enemy in enemyList)
        {
            if ((!isAlive || enemy.isDead) && enemy.target == this)
            {
                enemy.target = null;
                enemy.isAttacking = false;
                enemyList.Remove(enemy);

                //allow enemy to keep moving towards the hub unless hub is destroyed.
                Debug.Log(enemy + " Forgeto " + gameObject);
            }
            else if (attacking.DistanceCheck(gameObject, enemy.gameObject, enemy.attackRange) && (enemy.target == null || enemy.target == this))
            {                
                if((enemy.Ranged == true && Pylon == true) || Hub == true)
                {
                    Debug.Log(enemy.gameObject + " met the requirements to attack " + gameObject);
                    enemy.target = this;
                    attacking.BasicEnemyDamageHeart(this, enemy);
                }
            }
        }

        if (!isAlive)
        {
            Debug.Log(gameObject + " Goneo");
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
            enemyList.Add(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<TempEnemy>() != null)
        {
            TempEnemy enemy = other.GetComponent<TempEnemy>();
            if (enemy.target == this)
                enemy.target = null;
            enemyList.Remove(enemy);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, GetComponent<CapsuleCollider>().radius);
    }
}
