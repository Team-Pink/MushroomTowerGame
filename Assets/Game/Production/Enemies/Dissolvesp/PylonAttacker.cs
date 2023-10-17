using UnityEngine;

public class PylonAttacker : Enemy
{
    [Header("Pylon Attacker Variables")]
    private Pylon targetPylon;

    //[SerializeField] Pylon targetBuilding;
    public float firingCone = 10;
    [SerializeField] float detectionRange = 15;
    [SerializeField, Range(0.1f, 1.0f)] float turnSpeed = 1;

    // garbage animation objects
    [SerializeField] GameObject bullet;
    [SerializeField, Range(0.1f, 1.0f)] float bulletSpeed;

    LayerMask mask = new();

    protected override void ApproachState()
    {
        base.ApproachState(); //move towards the hub (either gonna have at the start or the end of the function)

        //Checks for any pylons in range
        foreach (Collider collider in Physics.OverlapSphere(transform.position, detectionRange))
        {
            if (collider.GetComponent<Pylon>() == null)
                continue;
        
            Pylon pylon = collider.GetComponent<Pylon>();
        
            if (pylon.CurrentHealth > 0)
            {
                targetBuilding = pylon;
                targetPylon = pylon;
                state = EnemyState.Hunt;
                break;
            }
        }
    }

    protected override void HuntState()
    {
        rigidbody.velocity = Vector3.zero;
        if (targetPylon.CurrentHealth <= 0 || targetPylon == null)
        {
            targetBuilding = FindNewTarget();
        }

        //Move Pylon Attacker towards the target
        bool facingTarget = RotateToTarget(GetRotationToTarget());

        float distance = Vector3.Distance(transform.position, targetBuilding.transform.position);

        if (distance > attackRadius && !facingTarget)
            transform.position = Vector3.MoveTowards(transform.position, targetBuilding.transform.position, Time.deltaTime * Speed);
        else
            state = EnemyState.Attack;
    }

    protected override void AttackState()
    {
        if (targetBuilding == null)
        {
            ResetBullet();
            targetBuilding = FindNewTarget();
            return;
        }
        else if (targetBuilding is Hub)
        {
            base.AttackState(); // Attack Hub
            return;
        }

        Pylon pylonTarget = targetBuilding.GetComponent<Pylon>();

        //On Pylon Death or Deactivation
        if (pylonTarget.CurrentHealth <= 0)
        {
            ResetBullet();
            targetBuilding = FindNewTarget();
            return;
        }

        //Attacking the Pylon
        if (!attackInProgress)
        {
            AttackAudio();
            animator.SetTrigger("Attack");
            pylonTarget.CurrentHealth -= damage;
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

    Quaternion GetRotationToTarget() => Quaternion.LookRotation((targetBuilding.transform.position - transform.position).normalized);

    void FireBullet()
    {
        bullet.transform.position = Vector3.Lerp(bullet.transform.position, targetBuilding.transform.position + Vector3.up, Time.deltaTime * 2);
    }

    void ResetBullet()
    {
        bullet.transform.position = transform.position;
        bullet.SetActive(false);
    }

   Pylon FindNewTarget()
    {
        Pylon pylon = null;

        Collider[] collisions = Physics.OverlapSphere(transform.position, detectionRange, mask);

        if (collisions.Length < 1)
        {
            state = EnemyState.Approach;
            return pylon;
        }

        foreach (Collider collider in collisions)
        {
            if (collider.GetComponent<Pylon>().CurrentHealth > 0)
            {
                targetBuilding = collider.GetComponent<Pylon>();
                state = EnemyState.Hunt;
                break;
            }
        }

        if (targetBuilding != null)
            state = EnemyState.Hunt;
        else
            state = EnemyState.Approach;
        return pylon;
    }
}