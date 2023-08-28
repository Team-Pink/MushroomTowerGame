using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter
{
    public struct Target
    {
        public Vector3 position;
        public Enemy enemy;



        public Target(Vector3 targetPos, Enemy targetEnemy = null)
        {
            position = targetPos;
            enemy = targetEnemy;
        }
    }


    public float range = 10; // radius of range collider
    public float turnRate;
    public Transform transform;
    protected HashSet<Target> targetsInRange = new HashSet<Target>();
    public LayerMask enemyLayer;


    public void GetTargetsInRange()
    {
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, range, enemyLayer);
        if (enemyColliders.Length <= 0) return;
        if (targetsInRange != null) targetsInRange.Clear(); // as far as I have tried, using remove to take out targets that have left the range is impossible to make work without minimum 3 loops.
        foreach (Collider collider in enemyColliders)
        {
            targetsInRange.Add(new Target(collider.transform.position, collider.GetComponent<Enemy>()));
        }
        Debug.Log(targetsInRange.Count + " enemies in range of", transform);
    }


    public virtual HashSet<Target> AcquireTargets(int numTargets = 1)
    {
        return null;
    }
}