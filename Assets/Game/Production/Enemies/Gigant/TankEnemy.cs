using UnityEngine;

public class TankEnemy : Enemy
{
    [Header("Biote Specific Variables")]
    [SerializeField] int damageReduction = 1;

    private float halfHealthMark = 0;
    private bool hasArmour = true;

    public override void TakeDamage(float damage)
    {
        if (hasArmour)
        {
            if (damage < damageReduction) return; // Prevent damage less than damageReduction causing enemies to heal.

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
