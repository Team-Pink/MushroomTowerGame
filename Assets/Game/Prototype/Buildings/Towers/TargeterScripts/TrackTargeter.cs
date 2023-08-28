using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTargeter : Targeter
{
    public LayerMask layerMask;
    [SerializeField] private float minRange = 0.5f;
    public override HashSet<Target> AcquireTargets(int numTargets = 1)
    {
        HashSet<Target> targets = new HashSet<Target>();


        targets = GenerateTargetsInRange(100);
        //targets = FindNumTargetsInRange(5);

        return targets;
    }



    private bool OnTrack(Vector3 positionOfTarget)
    {
        RaycastHit hit;
        // Raycast to see if the first thing hit is the path or if something (a trap, an enemy, the ground) is in the way
        if (Physics.Raycast((positionOfTarget + (Vector3.up * 10)), Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            //if (hit.transform.)
            return false;
        }
        return true;    
    }

  


    private HashSet<Target> GenerateTargetsInRange(int attemptNum)
    {
        HashSet<Target> targets = new HashSet<Target>();

        for (int i = 0; i < attemptNum; i++)
        {
            Vector3 randpos = RandomPosition();// get a random point in the bounds of the tower's range
           if (OnTrack(randpos))// check if on track
            {
                targets.Add(new Target(randpos));
                Debug.DrawLine(transform.position, randpos, Color.red, 0.02f);
            }

        }

        Debug.LogWarning("No function exists for checking pre-existing traps at target positions");
        return targets;
    }

    private HashSet<Target> FindNumTargetsInRange(int TrapNum)
    {
        int attemptNum = 1000;
        HashSet<Target> targets = new HashSet<Target>();
        int TargetNum = 0;
        // get a random point in the bounds of the tower's range

        for (int i = 0; i < attemptNum; i++)
        {
            Vector3 randpos = RandomPosition();// get a random point in the bounds of the tower's range
            if (OnTrack(randpos))// check if on track
            {
                targets.Add(new Target(randpos));
                Debug.DrawLine(transform.position, randpos, Color.blue, 0.02f);
                TargetNum++;
                
            }
            if (TargetNum >= TrapNum)
                    break;
        }


        // check to see if there is already a trap there
        // if yes move it the least distance possible away and check if still OnTrack()
        return targets;
    }

    private Vector3 RandomPosition()
    {
        float theta = Random.Range(0, 360);
        float radius = Random.Range(minRange, range);

        Vector3 offset = transform.position + new Vector3(radius * Mathf.Cos(theta), 0.0f, radius * Mathf.Sin(theta));

        return offset;
    }

    // what to do if there is no track in range
}
