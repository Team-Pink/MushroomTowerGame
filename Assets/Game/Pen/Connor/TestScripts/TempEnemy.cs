using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEnemy : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 5f;
    public float timeBetweenAttacks = 5f;
    public float attackDamage = 1f;
    public bool isAttacking = false;

    public float health = 11f;
    public bool isDead = false;

    public TempTower target = null;

    public bool Ranged = false;
    public bool Melee = false;

    public List<TempTower> detected = new();


    //public Animation Animation;

    private void Update()
    {
        if (health <= 0)
        {
            gameObject.SetActive(false);
            isDead = true;
            isAttacking = false;
            target = null;

            foreach (TempTower temp in detected)
            {
                temp.enemyList.Remove(this);
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
