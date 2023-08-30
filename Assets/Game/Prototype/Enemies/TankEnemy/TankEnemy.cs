using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemy : Enemy
{
    int halfHealthMark = 0;
    bool hasArmour = true;
    
    protected override void CustomAwakeEvents()
    {
        halfHealthMark = health / 2;
    }

    void Update()
    {
        if (!isDead)
            Playing();
    }

    protected override void Playing()
    {
        base.Playing();
    }

    protected override void AttackHub()
    {
        base.AttackHub();

        if (!attackInProgress && !attackCoolingDown)
            TakeDamage(damage);
    }

    public override void TakeDamage(int damage)
    {
        if (hasArmour)
        {
            base.TakeDamage(damage - 1);
            if (health <= halfHealthMark)
            {
                hasArmour = false;
            }
        }
        else
        {
            base.TakeDamage(damage);
        }

    }
}
