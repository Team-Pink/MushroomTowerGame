using UnityEngine;

public class TankEnemy : Enemy
{
    [Header("Biote Specific Variables")]
    [SerializeField] int damageReduction = 1;

    private float halfHealthMark = 0;
    private bool hasArmour = true;

    public override void SpawnIn()
    {
        base.SpawnIn();

        hasArmour = true;
        halfHealthMark = health / 2;
    }

    public override void TakeDamage(float damage)
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
