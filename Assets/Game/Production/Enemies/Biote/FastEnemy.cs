using UnityEngine;

public class FastEnemy : Enemy
{
    [Header("Biote Specific Variables")]
    [SerializeField] float speedUpMultiplier = 3.0f;
    private bool damaged = false;

    public override void SpawnIn()
    {
        damaged = false;

        base.SpawnIn();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (!damaged && !Dead)
        {
            damaged = true;
            speedModifiers.Add(speedUpMultiplier);
        }
    }

    [ContextMenu("damage")]
    public void Damage()
    {
        TakeDamage(1);
    }

    protected override void AttackState()
    {
        base.AttackState();

        if (!attackInProgress && !attackCoolingDown)
        {
            //Explosion logic goes here
            TakeDamage(CurrentHealth);
            OnDeath();
        }    
    }
}
