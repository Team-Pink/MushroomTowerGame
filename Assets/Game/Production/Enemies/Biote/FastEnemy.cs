using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastEnemy : Enemy
{
    bool damaged = false;
    float speedBeforeDamaged = 0;
    [Header("Biote Specific Variables")]
    [SerializeField, Range(0.0f, 5.0f)] float speedAfterDamaged = 0;
    [SerializeField, Tooltip("animation time for Biote changing speed")] float speedChangeTime = 0;

    protected override void CustomAwakeEvents()
    {
        speedBeforeDamaged = speed;
    }

    void Update()
    {
        if (!Dead)
            Playing();
    }

    public override void SpawnIn()
    {
        damaged = false;
        speed = speedBeforeDamaged;

        base.SpawnIn();
    }

    protected override void Playing()
    {
        base.Playing();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (!damaged && !Dead)
        {
            damaged = true;
            StartCoroutine(ChangeSpeed());
        }
    }

    protected override void AttackHub()
    {
        base.AttackHub();

        if (!attackInProgress && !attackCoolingDown)
        {
            //Explosion logic goes here
            TakeDamage(CurrentHealth);
        }    
    }

    //This exists if you want to have it stop for its animation or keep 'speedChangeTime' as 0 to have it continue moving
    private IEnumerator ChangeSpeed()
    {
        speed = 0;

        //play animation here
        
        yield return new WaitForSeconds(speedChangeTime);

        speed = speedAfterDamaged;
    }
}
