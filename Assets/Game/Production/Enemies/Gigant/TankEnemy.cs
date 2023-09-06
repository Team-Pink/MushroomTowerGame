using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemy : Enemy
{
    [Space]
    int damageReduction = 1;

    int halfHealthMark = 0;
    bool hasArmour = true;
    
    protected override void CustomAwakeEvents()
    {
        halfHealthMark = health / 2;
    }

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
        base.AttackHub();
    }

    public override void TakeDamage(int damage)
    {
        if (hasArmour)
        {
            base.TakeDamage(damage - damageReduction);

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
