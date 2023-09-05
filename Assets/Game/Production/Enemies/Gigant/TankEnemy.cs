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

    public override IEnumerator TakeDamage(int damage)
    {
        if (hasArmour)
        {
            StartCoroutine(base.TakeDamage(damage - damageReduction));

            if (health <= halfHealthMark)
            {
                hasArmour = false;
            }
        }
        else
        {
            StartCoroutine(base.TakeDamage(damage));
        }
        yield return null;
    }
}
