using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularEnemy : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Dead)
            Playing();
    }

    protected override void Playing()
    {
        base.Playing();
    }

    protected override void AttackHub()
    {
        if (!attackInProgress && !attackCoolingDown)
        {
            StartCoroutine(TakeDamage(damage, attackDelay));
        }

        base.AttackHub();
    }
}
