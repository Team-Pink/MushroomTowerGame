using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{

    // So how do I do this...

    public HashSet<GameObject> inRangeEnemies = new HashSet<GameObject>();
    GameObject targetedEnemy;

    public float turnSpeed = 5;
    public float magnitudeDelta = 0;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (targetedEnemy)
        {

            Vector3.RotateTowards(transform.rotation.eulerAngles, targetedEnemy.transform.position, turnSpeed, magnitudeDelta );
            Debug.Log("Enemy targeted.");
        }
    }
    /*
    public void PickPriorityTarget()
    {
        int bestScoreSoFar = -1;
        GameObject bestTargetSoFar;

        foreach(GameObject thisEnemy in inRangeEnemies)
        {
            int thisScore = SomeFunctionToEvaluateEnemy(thisEnemy);
            if (thisScore > bestScoreSoFar)
            {
                bestScoreSoFar = thisScore;
                bestTargetSoFar = thisEnemy;
            }
        }

        targetedEnemy = bestTargetSoFar;
    }*/

    private void OnTriggerEnter(Collider other)
    {
               if (inRangeEnemies.Count == 0)
        {
            targetedEnemy = other.gameObject;
        }
        inRangeEnemies.Add(other.gameObject);
        Debug.Log(inRangeEnemies.Count + " enemies in range right now.");
    }

    private void OnTriggerExit(Collider other)
    {
        inRangeEnemies.Remove(other.gameObject);

    }

}
