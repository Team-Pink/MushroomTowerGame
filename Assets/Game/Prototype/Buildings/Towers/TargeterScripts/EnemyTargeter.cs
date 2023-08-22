using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

/// <summary>
/// Priority Enemies closest to the tower.
/// </summary>
public class CloseTargeter : EnemyTargeter
{
    protected HashSet<Target> GetTargets(int numTargets = 1) { return base.AcquireTargets(numTargets); }
    protected override bool PrioritiseTargets(Target targetInRange, Target storedTarget)
    {
        // if target in range distance is less swap
        return (Vector3.Distance(targetInRange.position, transform.position) < Vector3.Distance(storedTarget.position, transform.position)) ;
    }
}
/// <summary>
/// Priority enemy with the most surrounding enemies in a radius around them.
/// </summary>
public class ClusterTargeter : EnemyTargeter
{
    protected HashSet<Target> GetTargets(int numTargets = 1) { return base.AcquireTargets(numTargets); }
    protected override bool PrioritiseTargets(Target targetInRange, Target storedTarget)
    {
        Debug.Log("this Targeter cannot be implemented efficiently without the neighbourhood of flocking behaviour");
        // if neighboorhoud is bigger swap
        // return (targetInRange.enemy.neighbourhood.count > storedTarget.enemy.neighbourhood.count);
        Debug.Log(LayerMask.GetMask("Enemy") + " is the target physics layermask");
        // for now generate the neighbour hood myself using a layer mask which has been known to end badly
        return (Physics.OverlapSphere(targetInRange.position, 1.5f,LayerMask.GetMask("Enemy")).Length > Physics.OverlapSphere(storedTarget.position, 1.5f, LayerMask.GetMask("Enemy")).Length);
    }
}
/// <summary>
/// Priority enemies with higher speed.
/// </summary>
public class FastTargeter : EnemyTargeter
{
    protected HashSet<Target> GetTargets(int numTargets = 1) { return base.AcquireTargets(numTargets); }
    protected override bool PrioritiseTargets(Target targetInRange, Target storedTarget)
    {
        return (targetInRange.enemy.Speed > storedTarget.enemy.Speed);
    }
}

/// <summary>
/// Priority enemies with higher health.
/// </summary>
public class StrongTargeter : EnemyTargeter
{
    protected HashSet<Target> GetTargets(int numTargets = 1) { return base.AcquireTargets(numTargets); }
    protected override bool PrioritiseTargets(Target targetInRange, Target storedTarget)
    {
        return (targetInRange.enemy.health > storedTarget.enemy.health);
    }
}
