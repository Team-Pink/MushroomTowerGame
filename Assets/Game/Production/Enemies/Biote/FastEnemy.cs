using UnityEngine;

public class FastEnemy : Enemy
{
    [Header("Biote Specific Variables")]
    [SerializeField] float speedUpMultiplier = 3.0f;
    private bool damaged = false;

    protected override void Awake()
    {
        base.Awake();
        animator.SetBool("Crawling", true);
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (!damaged && !Dead)
        {
            damaged = true;
            speedModifiers.Add(speedUpMultiplier);

            animator.SetBool("Crawling", false);
            animator.SetBool("Spinning", true);
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
            AttackAudio();
            TakeDamage(CurrentHealth);
            OnDeath();
        }    
    }
}
