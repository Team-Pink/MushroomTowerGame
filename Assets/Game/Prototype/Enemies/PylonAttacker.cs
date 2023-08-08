using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PylonAttacker : Enemy
{
    [SerializeField] private Pylon target;
    private Quaternion startingRotation;
    private bool lockedOn = false;
    public float firingCone = 10;
    [SerializeField, Range(0.1f, 1.0f)] private float turnSpeed = 1;

    private void Update()
    {
        AttackPylon();
        base.Playing();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pylon"))
        {
            target = other.GetComponent<Pylon>();
            if (target.PylonHealth <= 0)
            {
                target = null;
                return;
            }

            // stop movement
            speed = 0;
            Debug.Log("encountered" + other);
        }
    }

    void AttackPylon()
    {
        if (target)
        {
            // rotate to face pylon
            RotateToTarget(GetRotationToTarget());
            if (lockedOn && !attackInProgress)
            {
                if (target.PylonHealth <= 0)
                {
                    lockedOn = false;
                    target = null;
                    //turn back on the path this should be corrected by the flocking/steering behavior when it's implemented
                    transform.rotation = startingRotation;
                    // resume travelling
                    speed = 1;
                    return;
                }
                // trigger pylon attack animation
                target.PylonHealth -= 1;
                attackInProgress = true;
            }
            else
            {
                elapsedCooldown += Time.deltaTime;

                if (elapsedCooldown >= attackCooldown)
                {
                    attackInProgress = false;
                    elapsedCooldown = 0;
                }
            }
        }
    }

    void RotateToTarget(Quaternion lookTarget)  // this should be overridden in child classes
    {
        if (!lockedOn && Quaternion.Angle(transform.rotation, lookTarget) < firingCone)
            lockedOn = true;

        transform.rotation = Quaternion.Slerp(transform.rotation, lookTarget, turnSpeed);
    }

    Quaternion GetRotationToTarget() => Quaternion.LookRotation((target.transform.position - transform.position).normalized);

    
}
