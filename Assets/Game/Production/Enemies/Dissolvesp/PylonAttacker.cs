using UnityEngine;

public class PylonAttacker : Enemy
{
    [Header("Pylon Attacker Variables")]

    [SerializeField] Pylon target;
    public float firingCone = 10;
    [SerializeField] float detectionRange = 15;
    [SerializeField, Range(0.1f, 1.0f)] float turnSpeed = 1;

    // garbage animation objects
    [SerializeField] GameObject bullet;
    [SerializeField, Range(0.1f, 1.0f)] float bulletSpeed;

    LayerMask mask = new();

    public override void SpawnIn()
    {
        mask = LayerMask.GetMask("Pylon");

        base.SpawnIn();
    }



    protected override void ApproachState()
    {
        base.ApproachState(); //move towards the hub (either gonna have at the start or the end of the function)

        //Checks for any pylons in range
        foreach (Collider collider in Physics.OverlapSphere(transform.position, detectionRange))
        {
            if (collider.GetComponent<Pylon>() == null)
                continue;

            if (collider.GetComponent<Pylon>().CurrentHealth > 0)
            {
                target = collider.GetComponent<Pylon>();
                state = EnemyState.Hunt;
                break;
            }
        }
    }

    protected override void HuntState()
    {
        rigidbody.velocity = Vector3.zero;
        if (target.CurrentHealth <= 0)
        {
            Collider[] collisions = Physics.OverlapSphere(transform.position, detectionRange, mask);

            if (collisions.Length < 1)
            {
                state = EnemyState.Approach;
                target = null;
                ResetBullet();
                return;
            }

            foreach (Collider collider in collisions)
            {
                if (collider.GetComponent<Pylon>().CurrentHealth > 0)
                {
                    target = collider.GetComponent<Pylon>();
                    break;
                }
            }
        }

        //Move Pylon Attacker towards the target
        bool facingTarget = RotateToTarget(GetRotationToTarget());

        if (Vector3.Distance(transform.position, target.transform.position) > attackRadius)
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * Speed);

        if (Vector3.Distance(transform.position, target.transform.position) <= attackRadius && facingTarget)
        {
            state = EnemyState.Attack;
        }
    }

    protected override void AttackState()
    {
        if (target == null)
        {
            base.AttackState(); // Attack Hub
            return;
        }

        //On Pylon Death or Deactivation
        if (target.CurrentHealth <= 0)
        {
            target = null;
            ResetBullet();

            Collider[] collisions = Physics.OverlapSphere(transform.position, detectionRange, mask);

            if (collisions.Length < 1)
            {
                state = EnemyState.Approach;
                return;
            }

            foreach (Collider collider in collisions)
            {   
                if (collider.GetComponent<Pylon>().CurrentHealth > 0)
                {
                    target = collider.GetComponent<Pylon>();
                    state = EnemyState.Hunt;
                    break;
                }
            }

            if (target != null)
                state = EnemyState.Hunt;
            else
                state = EnemyState.Approach;

            return;
        }

        //Attacking the Pylon
        if (!attackInProgress)
        {
            AttackAudio();
            animator.SetTrigger("Attack");
            target.CurrentHealth -= damage;
            attackInProgress = true;
            bullet.SetActive(true);
        }
        else
        {
            elapsedCooldown += Time.deltaTime;
            FireBullet();

            if (elapsedCooldown < attackCooldown) return;

            bullet.SetActive(false);
            ResetBullet();
            attackInProgress = false;
            elapsedCooldown = 0;
        }
    }



    bool RotateToTarget(Quaternion lookTarget)  // this should be overridden in child classes
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, lookTarget, turnSpeed);

        return Quaternion.Angle(transform.rotation, lookTarget) < firingCone;
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