using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{

    // So how do I do this...

    public HashSet<GameObject> inRangeEnemies = new HashSet<GameObject>();
    protected LochTestEnemy targetEnemy;

    public bool pylonActive = true;
    public float damage = 100;
    public float firingInterval = 3;
    public float firingClock = 2;
    //public float bulletSpeed;

    public float turnSpeed = 5;
    public float magnitudeDelta = 0;


    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        firingClock += Time.fixedDeltaTime;
        if (targetEnemy)
        {
            // rotate turret to targetted enemy
            transform.Rotate(Vector3.up, turnSpeed * Time.fixedDeltaTime);

            if (firingClock > firingInterval)
                Attack();
        }


    }


    private void OnTriggerEnter(Collider other)
    {
        inRangeEnemies.Add(other.gameObject);

        if (!targetEnemy) PickPriorityTarget();
    }

    private void OnTriggerExit(Collider other)
    {

         inRangeEnemies.Remove(other.gameObject);

        if (!targetEnemy) PickPriorityTarget();
    }

    private void Attack()
    {
        // do attack animation

        targetEnemy.health -= damage; Debug.Log(targetEnemy.gameObject.name + " has " + targetEnemy.health + " health remaining");
        if (targetEnemy.health <= 0)
        {
            inRangeEnemies.Remove(targetEnemy.gameObject);
            PickPriorityTarget();
        }

        firingClock = 0;
    }


    public void PickPriorityTarget()
    {
        if (inRangeEnemies.Count <= 0)
        {
            targetEnemy = null;
            return;
        }
        int bestScoreSoFar = -1;
        GameObject bestTargetSoFar = new GameObject(); // I don't know why I have to assign this something but it doesn't work otherwise
        GameObject deleteThis = bestTargetSoFar; // so feel free to roll over this if you know how to do better.

        foreach (GameObject thisEnemy in inRangeEnemies)
        {
            int thisScore = TargetingAlgorithm(thisEnemy);
            if (thisScore > bestScoreSoFar)
            {
                bestScoreSoFar = thisScore;
                bestTargetSoFar = thisEnemy;
            }
        }
        Destroy(deleteThis);

        targetEnemy = bestTargetSoFar.GetComponent<LochTestEnemy>();
    }

    int TargetingAlgorithm(GameObject enemy)
    {
        int score = Random.RandomRange(1, 10);
        return score;
    }
}
