using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter
{

    public float range = 10; // radius of range collider
    public Transform transform;
    protected HashSet<Target> targetsInRange = new HashSet<Target>();
    public LayerMask enemyLayer;


    public virtual HashSet<Target> AcquireTargets(int numTargets = 1)
    {

        return null;
    }
}