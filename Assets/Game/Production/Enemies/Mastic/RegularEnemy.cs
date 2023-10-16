public class RegularEnemy : Enemy
{
    bool hasAttacked;
    protected override void AttackState()
    {
        base.AttackState();

        if (attackInProgress) hasAttacked = false;
        if (!attackCoolingDown && !attackInProgress)
        {
            if (hasAttacked) return;

            TakeDamage(damage);
            if(CheckIfDead()) OnDeath();
            hasAttacked = true;
        }
    }
}