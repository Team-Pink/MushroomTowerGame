using UnityEngine;

public class PylonAttacker : Enemy
{
    [SerializeField] private Pylon target;
    private bool lockedOn = false;
    public float firingCone = 10;
    [SerializeField, Range(0.1f, 1.0f)] private float turnSpeed = 1;

    // garbage animation objects
    [SerializeField] GameObject bullet;
    [SerializeField, Range(0.1f, 1.0f)] private float bulletSpeed;

    private void Update()
    {
        if (!Dead)
            Playing();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pylon"))
        {
            target = other.GetComponent<Pylon>();
            if (target.CurrentHealth <= 0)
            {
                target = null;
                return;
            }

            // stop movement
            speed = 0;
            Debug.Log("encountered" + other);
        }
    }

    protected override void Playing()
    {
        AttackPylon();
        base.Playing();
    }

    void AttackPylon() // The reason this is as terrible as it is is because I had to break up the order to add in FireBullet.
    {
        if (target)
        {
            if (target.CurrentHealth <= 0)
            {

                //turn back on the path this should be corrected by the flocking/steering behavior when it's implemented
                // resume travelling
                speed = 1;
                lockedOn = false;
                target = null;
                ResetBullet();
                return;
            }
            // rotate to face pylon
            RotateToTarget(GetRotationToTarget());
            if (lockedOn && !attackInProgress)
            {

                // trigger pylon attack animation              
                target.CurrentHealth -= 1;
                attackInProgress = true;
            }
            else
            {
                elapsedCooldown += Time.deltaTime;
                bullet.SetActive(lockedOn);
                FireBullet();
                if (elapsedCooldown >= attackCooldown)
                {
                    ResetBullet();
                    attackInProgress = false;
                    elapsedCooldown = 0;
                }
            }
        }
        else if (lockedOn)
        {
            speed = 1;
            lockedOn = false;
            target = null;
            ResetBullet();
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
