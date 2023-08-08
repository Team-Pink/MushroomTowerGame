using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PylonAttacker : Enemy
{
    Pylon target;

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
            if(target.PylonHealth <= 0)
            {
                target = null;
                return;
            }
            // stop movement
            speed = 0;
            // turn to face pylon
            Debug.Log("encountered" + other);
        }
    }

    void AttackPylon()
    {
        if (target)
        {
            if (!attackInProgress)
            {
                if (target.PylonHealth <= 0)
                {
                    target = null;
                    //turn back on the path
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

    // if (pylon health =< 0) return to path
}
