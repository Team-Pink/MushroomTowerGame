using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    public struct Target
    {
        public Vector3 position;
        public Enemy enemy;
    }
    public float range;
    public float turnRate;
    protected HashSet<Target> targetsInRange;

    protected virtual HashSet<Target> AcquireTargets(int numTargets = 1)
    {
        return new HashSet<Target>();
    }
}