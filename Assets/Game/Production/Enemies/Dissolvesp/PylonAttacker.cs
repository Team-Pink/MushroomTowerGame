using UnityEngine;

public class PylonAttacker : Enemy
{
    [SerializeField] Pylon target;
    private bool lockedOn = false;
    public float firingCone = 10;
    [SerializeField, Range(0.1f, 1.0f)] float turnSpeed = 1;

    // garbage animation objects
    [SerializeField] GameObject bullet;
    [SerializeField, Range(0.1f, 1.0f)] float bulletSpeed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pylon"))
        {
            target = other.GetComponent<Pylon>();
            if (target == null || target.CurrentHealth <= 0)
            {
                target = null;
                return;
            }

            state = EnemyState.Attack;
            Debug.Log("encountered" + other);
        }
    }

    protected override void AttackState()
    {
        if (target == null)
        {
            base.AttackState(); // Attack Hub
            return;
        }

        // Otherwise Attack Pylon

        if (target.CurrentHealth <= 0)
        {
            state = EnemyState.Approach;
            lockedOn = false;
            target = null;
            ResetBullet();
            return;
        }

        RotateToTarget(GetRotationToTarget());
        if (lockedOn && !attackInProgress)
        {
            animator.SetTrigger("Attack");
            target.CurrentHealth -= 1;
            attackInProgress = true;
        }
        else
        {
            elapsedCooldown += Time.deltaTime;
            bullet.SetActive(lockedOn);
            FireBullet();

            if (elapsedCooldown < attackCooldown) return;

            ResetBullet();
            attackInProgress = false;
            elapsedCooldown = 0;
        }
    }

    void RotateToTarget(Quaternion lookTarget)  // this should be overridden in child classes
    {
        if (!lockedOn && Quaternion.Angle(transform.rotation, lookTarget) < firingCone)
            lockedOn = true;

        transform.rotation = Quaternion.Slerp(transform.rotation, lookTarget, turnSpeed);
    }

    Quaternion GetRotationToTarget() => Quaternion.LookRotation((target.transform.position - transform.position).normalized);

    void FireBullet()
    {
        bullet.transform.position = Vector3.Lerp(bullet.transform.position, target.transform.position + Vector3.up, Time.deltaTime * 2);
    }

    void ResetBullet()
    {
        bullet.transform.position = transform.position;
        bullet.SetActive(false);
    }
}