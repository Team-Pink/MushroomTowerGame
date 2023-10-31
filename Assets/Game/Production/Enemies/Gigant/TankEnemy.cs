using UnityEngine;

public class TankEnemy : Enemy
{
    [Header("Bulk Specific Variables")]
    [SerializeField] int damageReduction = 1;

    private float halfHealthMark = 0;
    private bool hasArmour = true;

    [SerializeField] GameObject armour;
    [SerializeField] ParticleSystem armourParticle;

    protected override void Awake()
    {
        halfHealthMark = MaxHealth * 0.5f;
        base.Awake();
    }

    public override void TakeDamage(float damage)
    {
        if (hasArmour)
        {
            if (damage < damageReduction) return; // Prevent damage less than damageReduction causing enemies to heal.

            base.TakeDamage(damage - damageReduction);

            if (health <= halfHealthMark)
            {
                RemoveArmour();
            }
        }
        else
        {
            base.TakeDamage(damage);
        }
    }

    private void RemoveArmour()
    {
        // deactivate mesh
        armour.SetActive(false);

        // play particle
        armourParticle.Play();

        hasArmour = false;
    }
}
