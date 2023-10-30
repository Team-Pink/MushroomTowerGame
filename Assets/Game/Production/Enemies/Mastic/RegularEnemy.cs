public class RegularEnemy : Enemy
{
    protected override void AttackState()
    {
        if (!attackInProgress && !attackCoolingDown)
        {
            TakeDamage(damage);
            if(CheckIfDead()) OnDeath();
        }

        base.AttackState();
    }
}