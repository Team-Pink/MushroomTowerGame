using System.Collections.Generic;
using UnityEngine;

public class TrackTargeter : Targeter
{
    public float trapRadius = 1;
    private const int maxTargets = 1000;
    public LayerMask layerMask;
    
    [SerializeField] private float minRange = 2.0f;
    public override HashSet<Target> AcquireTargets(int numTargets = 1)
    {
        HashSet<Target> targets;

        targets = GenerateTargetsInRange(maxTargets);

        return targets;
    }

    private HashSet<Target> GenerateTargetsInRange(int attemptNum)
    {
        HashSet<Target> targets = new();

        for (int i = 0; i < attemptNum; i++)
        {
            Vector3 randpos = RandomPosition();
            targets.Add(new Target(randpos));
            //Debug.DrawLine(transform.position, randpos, Color.red, 0.02f);

        }
        return targets;
    }

    private Vector3 RandomPosition()
    {
        float theta = Random.Range(0, 360);
        float radius = Random.Range(minRange, range);

        Vector3 offset = transform.position + new Vector3(radius * Mathf.Cos(theta), 0.0f, radius * Mathf.Sin(theta));

        return offset;
    }
}
