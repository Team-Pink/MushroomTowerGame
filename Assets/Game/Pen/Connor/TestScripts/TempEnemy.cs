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


    //public Animation Animation;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
