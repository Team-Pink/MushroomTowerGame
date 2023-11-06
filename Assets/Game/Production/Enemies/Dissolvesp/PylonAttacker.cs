using System.Collections;
using UnityEngine;

public class PylonAttacker : Enemy
{
    [Header("Node Attacker Variables")]
    private Node targetNode;

    public float firingCone = 10;
    private float attackDuration;
    [SerializeField] float detectionRange = 15;
    [SerializeField, Range(0.1f, 1.0f)] float turnSpeed = 1;

    // garbage animation objects
    [SerializeField] GameObject bulletPrefab;
    [SerializeField, Range(0.1f, 1.0f)] float bulletSpeed;

    LayerMask mask = new();

    protected override void ApproachState()
    {
        base.ApproachState(); //move towards the Meteor (either gonna have at the start or the end of the function)

        //Checks for any Nodes in range
        foreach (Collider collider in Physics.OverlapSphere(transform.position, detectionRange))
        {
            if (collider.GetComponent<Node>() == null)
                continue;

            Node node = collider.GetComponent<Node>();

            if (node.CurrentHealth > 0)
            {
                targetBuilding = node;
                targetNode = node;
                state = EnemyState.Hunt;
                break;
            }
        }
    }

    protected override void HuntState()
    {
        rigidbody.velocity = Vector3.zero;
        if (targetNode.CurrentHealth <= 0 || targetNode == null)
        {
            targetBuilding = FindNewTarget();
        }

        //Move Node Attacker towards the target
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
            targetBuilding = FindNewTarget();
            return;
        }
        else if (targetBuilding is Meteor)
        {
            base.AttackState(); // Attack Meteor
            return;
        }

        //On Node Death or Deactivation
        if (targetNode.CurrentHealth <= 0)
        {
            targetBuilding = FindNewTarget();
            return;
        }

        //Attacking the Node
        if (!attackInProgress)
        {
            AttackAudio();
            animator.SetTrigger("Attack");
            FireBullet();
            StartCoroutine(DamageNode());
            attackInProgress = true;

        }
        else
        {
            elapsedCooldown += Time.deltaTime;
            

            if (elapsedCooldown < attackCooldown) return;

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

        Bullet bulletRef = UnityEngine.Object.Instantiate(bulletPrefab, transform.position + Vector3.up * 2, Quaternion.identity).GetComponent<Bullet>();
        bulletRef.timeToTarget = attackDuration = Vector3.Distance(transform.position, targetNode.transform.position) / bulletSpeed; ;
        bulletRef.InitializeNoTrackParabolaBullet(targetNode.transform.position);

        //bullet.transform.position = Vector3.Lerp(bullet.transform.position, targetBuilding.transform.position + Vector3.up, Time.deltaTime * 2);
    }

    Node FindNewTarget()
    {
        Node node = null;

        Collider[] collisions = Physics.OverlapSphere(transform.position, detectionRange, mask);

        if (collisions.Length < 1)
        {
            state = EnemyState.Approach;
            return node;
        }

        foreach (Collider collider in collisions)
        {
            if (collider.GetComponent<Node>().CurrentHealth > 0)
            {
                targetBuilding = collider.GetComponent<Node>();
                state = EnemyState.Hunt;
                break;
            }
        }

        if (targetBuilding != null)
            state = EnemyState.Hunt;
        else
            state = EnemyState.Approach;
        return node;
    }

    public IEnumerator DamageNode()
    {
        yield return new WaitForSeconds(attackDuration);
        targetNode.CurrentHealth -= damage;
    }
}