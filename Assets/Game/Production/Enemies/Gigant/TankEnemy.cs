using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemy : Enemy
{
    [Space]
    int damageReduction = 1;

    float halfHealthMark = 0;
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

    public override void SpawnIn()
    {
        hasArmour = true;

        base.SpawnIn();
    }

    protected override void Playing()
    {
        base.Playing();
    }

    protected override void AttackHub()
    {
        base.AttackHub();
    }

    public override IEnumerator TakeDamage(float damage, float delay)
    {
        if (hasArmour)
        {
            StartCoroutine(base.TakeDamage(damage - damageReduction, delay));

            if (health <= halfHealthMark)
            {
                hasArmour = false;
            }
        }
        else
        {
            StartCoroutine(base.TakeDamage(damage, delay));
        }
        yield return null;
    }
}
