using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Targeter
{

    public float turnRate = 0;
    public float range = 10;
    public Transform transform;
    protected HashSet<Target> targetsInRange = new();
    public LayerMask enemyLayer;


    public virtual HashSet<Target> AcquireTargets(int numTargets = 1)
    {
        return null;
    }
}