using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTargeter : Targeter
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float minRange = 0.5f;
    protected override HashSet<Target> AcquireTargets(int numTargets = 1)
    {
        HashSet<Target> targets = new HashSet<Target>();


        // Okay so 

        return targets;
    }


    private bool OnTrack(Vector3 positionOfTarget)
    {
        // Raycast to see if the first thing hit is the path or if something (a trap, an enemy, the ground) is in the way
        if (Physics.Raycast((positionOfTarget + (Vector3.up * 10)), Vector3.down, Mathf.Infinity, layerMask))
        {
            return true;
        }
        return false;
    }

    private HashSet<Target> GenerateTargetsInRange(int attemptNum)
    {
        Target target;
        // get a random point in the bounds of the tower's range
        Vector3 randpos = RandomPosition();

        // check if on track

        // check to see if there is already a trap there
        // if yes move it the least distance possible away and check if still OnTrack()
        return target;
    }

    private HashSet<Target> FindNumTargetsInRange(int TrapNum)
    {
        Target target;
        // get a random point in the bounds of the tower's range
        Vector3 randpos = RandomPosition();

        // check if on track

        // check to see if there is already a trap there
        // if yes move it the least distance possible away and check if still OnTrack()
        return target;
    }

    private Vector3 RandomPosition()
    {
        float theta = Random.Range(0, 360);
        float radius = Random.Range(minRange, range);

        Vector3 offset = new Vector3( radius * Mathf.Cos(theta), 0.0f, radius * Mathf.Sin(theta));

        return offset;
    }

    // what to do if there is no track in range
}
