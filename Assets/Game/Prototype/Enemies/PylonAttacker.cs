using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PylonAttacker : Enemy
{
    Pylon target;

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pylon"))
        {
            target = other.GetComponent<Pylon>();

            Debug.Log("encountered" + other);
        }
    }

    void AttackPylon()
    {
        if (target)
        {
            if (!attackInProgress)
            {
                // stop movement and turn to face pylon
                // trigger pylon attack animation
                target.PylonHealth -= 1;
                if (target.PylonHealth <= 0)
                {
                    target = null;
                    //turn back on the path
                    // resume travelling
                }
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
        else
            return;

    }

    // if (pylon health =< 0) return to path
}
