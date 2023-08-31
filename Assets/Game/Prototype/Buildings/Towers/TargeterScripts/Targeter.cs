using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter
{

    public float turnRate = 5;
    public float firingCone = 20;
    public float range = 10; // radius of range collider
    public Transform transform;
    protected HashSet<Target> targetsInRange = new HashSet<Target>();
    public LayerMask enemyLayer;


    public virtual HashSet<Target> AcquireTargets(int numTargets = 1)
    {

        return null;
    }
}