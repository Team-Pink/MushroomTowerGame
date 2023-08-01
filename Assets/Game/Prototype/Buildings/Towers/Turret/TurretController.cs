using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{

    // So how do I do this...

    //Enemy catalouging
    public HashSet<GameObject> inRangeEnemies = new();
    public GameObject targetGameObject;
    protected Enemy targetEnemy;

    // bad firing animation
    public GameObject bullet;
    Vector3 bulletSpawn1;
    Vector3 bulletSpawn2;
    bool barrelAlternate;

    // pylon data
    public bool pylonActive = true;
    public int storedExperience;

    // tower values
    public float damage = 100;
    public float firingInterval = 3;
    private float firingClock = 2;
    public float turnSpeed = 2;
    public float firingCone = 20;
    public bool lockedOn = false;

    void Start()
    {
        bulletSpawn1 = transform.GetChild(1).transform.localToWorldMatrix.GetPosition();
        bulletSpawn2 = transform.GetChild(2).transform.localToWorldMatrix.GetPosition();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        firingClock += Time.fixedDeltaTime;
        if (targetEnemy)
        {
            // rotate turret to targetted enemy
            RotateToTarget();
            if (targetEnemy.Dead())
            {
                // take enemy experience
                storedExperience += targetEnemy.expValue;
                targetEnemy.expValue = 0;
                // run enemy death function
                targetEnemy.OnDeath();
                // remove it from targets and retarget
                inRangeEnemies.Remove(targetGameObject);
                PickPriorityTarget();
            }

            if (firingClock > firingInterval && lockedOn)
                Attack();
        }    
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            inRangeEnemies.Add(other.gameObject);
        }

        if (targetEnemy is null) PickPriorityTarget();
    }

    private void OnTriggerExit(Collider other)
    {
        inRangeEnemies.Remove(other.gameObject);
        if (targetGameObject == other.gameObject) // immediately remove object from the target if it is the current target.
        {
            targetGameObject = null;
            targetEnemy = null;
        }
        PickPriorityTarget();
    }

    private void Attack()
    {
        // do attack animation

        if (barrelAlternate)
        {
            Instantiate(bullet, bulletSpawn1, Quaternion.identity);
        }
        else
        {
            Instantiate(bullet, bulletSpawn2, Quaternion.identity);
        }
        barrelAlternate = !barrelAlternate;

        targetEnemy.health -= (int)damage;
        if (targetEnemy.health <= 0)
        {
            inRangeEnemies.Remove(targetEnemy.gameObject);
            PickPriorityTarget();
        }

        firingClock = 0;
    }


    public void PickPriorityTarget()
    {
        lockedOn = false;
        if (inRangeEnemies.Count <= 0)
        {
            targetGameObject = null;
            targetEnemy = null;
            return;
        }
        int bestScoreSoFar = -1;
        GameObject bestTargetSoFar = new(); // I don't know why I have to assign this something but it doesn't work otherwise
        GameObject deleteThis = bestTargetSoFar; // so feel free to roll over this if you know how to do better.

        foreach (GameObject thisEnemy in inRangeEnemies)
        {
            int thisScore = TargetingAlgorithm();
            if (thisScore > bestScoreSoFar)
            {
                bestScoreSoFar = thisScore;
                bestTargetSoFar = thisEnemy;
            }
        }
        Destroy(deleteThis);
        targetGameObject = bestTargetSoFar;
        targetEnemy = bestTargetSoFar.GetComponent<Enemy>();
    }

    int TargetingAlgorithm()
    {
        return Random.Range(0, 10);
    }

    void RotateToTarget()
    {
        Vector3 lookDirection = (targetGameObject.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

        if (!lockedOn && Quaternion.Angle(transform.rotation, lookRotation) < firingCone)
            lockedOn = true;

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }
}
