using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyTargeter : Targeter
{
    protected override HashSet<Target> AcquireTargets(int numTargets = 1)
    {        
        HashSet<Target> targets = new HashSet<Target>();
        if (targetsInRange.Count < numTargets) // early out if less targets than numTargets.
        {
            targets = targetsInRange;
            return targets;
        }

        foreach (Target target in targetsInRange)
        {
            if (targets.Count < numTargets)
                targets.Add(target);
            else
            {
                foreach (Target storedTarget in targets)
                {

                    if (PrioritiseTargets(target,storedTarget))
                    {
                        targets.Remove(storedTarget);
                        targets.Add(target);
                    }
                }
            }
        }
        return targets;

    }
    protected abstract bool PrioritiseTargets(Target targetInRange, Target storedTarget);
}

public class CloseTargeter : EnemyTargeter
{
    protected override HashSet<Target> PrioritiseTargets(Target targetInRange, Target storedTarget)
    {                
        return if (Vector3.Distance(target.position, transform.position) < Vector3.Distance(storedTarget.position, transform.position);
    }
}

public class ClusterTargeter : EnemyTargeter
{
    protected override HashSet<Target> PrioritiseTargets(Target targetInRange, Target storedTarget)
    {

    }
}
public class FastTargeter : EnemyTargeter
{
    protected override HashSet<Target> PrioritiseTargets(Target targetInRange, Target storedTarget)
    {
        HashSet<Target> targets = new HashSet<Target>();

        return targets;
    }
}
public class StrongTargeter : EnemyTargeter
{
    protected override HashSet<Target> PrioritiseTargets(Target targetInRange, Target storedTarget)
    {
        HashSet<Target> targets = new HashSet<Target>();

        return targets;
    }
}
