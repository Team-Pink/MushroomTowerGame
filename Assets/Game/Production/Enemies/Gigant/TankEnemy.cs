using UnityEngine;
using UnityEngine.EventSystems;

public class TankEnemy : Enemy
{
    [Header("Bulk Specific Variables")]
    [SerializeField] int damageReduction = 1;

    private float halfHealthMark = 0;
    private bool hasArmour = true;

    [SerializeField] GameObject armour;
    [SerializeField] ParticleSystem armourParticle;

    private void Awake()
    {
        halfHealthMark = MaxHealth * 0.5f;
    }

    public override void TakeDamage(float damage)
    {
        if (hasArmour)
        {
            if (damage < damageReduction) return; // Prevent damage less than damageReduction causing enemies to heal.

            base.TakeDamage(damage - damageReduction);

            if (health <= halfHealthMark)
                RemoveArmour();
        }
        else
            base.TakeDamage(damage);
    }

    private void RemoveArmour() { 
    
        // deactivate mesh
        armour.SetActive(false);

        // instance particle
        armourParticle.Play();

        hasArmour = false;
    }
}
