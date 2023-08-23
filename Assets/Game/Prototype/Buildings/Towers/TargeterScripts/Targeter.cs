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
    protected HashSet<Target> targetsInRange;

    public virtual void GetTargetsInRange()
    {

    }


    public virtual HashSet<Target> AcquireTargets(int numTargets = 1)
    {
        return null;
    }
}