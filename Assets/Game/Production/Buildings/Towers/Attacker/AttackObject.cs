using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackObject : MonoBehaviour
{
    public float delayToTarget;
    public int damage;
    public Enemy targetObject;
    public Tower originObject; 
    public AttackerType attackerType;

    // private Animator

    // Update is called once per frame
    void Update()
    {
        if(delayToTarget > 0)delayToTarget -= Time.deltaTime;
        else
        {
            // play impact animation
            if (targetObject.CheckIfDead())
            {

                // Do Area attack on target location

            }
            else
            {

            }

            // Destroy this
        }
    }
}
