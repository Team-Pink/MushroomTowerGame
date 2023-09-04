using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyTargeter : Targeter
{
    Quaternion defaultRotation = Quaternion.identity;

    public void GetTargetsInRange()
    {
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, range, enemyLayer);
        if (enemyColliders == null) return;
        targetsInRange.Clear();
        foreach (Collider collider in enemyColliders)
        {
            targetsInRange.Add(new Target(collider.transform.position, collider.GetComponent<Enemy>()));
        }
    }

    public override HashSet<Target> AcquireTargets(int numTargets = 1)
    {
        if (defaultRotation == Quaternion.identity)
            defaultRotation = Quaternion.Euler(0, 180, 0);

        HashSet<Target> targets = new();
        GetTargetsInRange();
        if (targetsInRange.Count == 0 || targetsInRange == null)
        {
            if (transform.rotation != defaultRotation)
                transform.rotation = defaultRotation;
            return null;
        }
        if (targetsInRange.Count <= numTargets) // early out if less targets than numTargets.
        {
            targets = targetsInRange;
            if (CheckRotation(targets))
                return targets;
        }

        foreach (Target target in targetsInRange)
        {
            if (targets.Count < numTargets)
                targets.Add(target);
            else
            {
                Target toRemove = target;
                bool swap = false;
                foreach (Target storedTarget in targets)
                {
                    if (PrioritiseTargets(toRemove, storedTarget))
                    {
                        toRemove = storedTarget;
                        swap = true;
                    }
                }

                if (swap)
                {
                    targets.Remove(toRemove);
                    targets.Add(target);
                }
            }
        }
        //foreach (Target target in targets) // for testing
        //{
        //    Debug.DrawLine(transform.position, target.position, Color.red, 0.02f);
        //}
        if (CheckRotation(targets))
            return targets;

        return null;

    }

    protected bool CheckRotation(HashSet<Target> targets)
    {
        // get the average position of the targets
        Vector3 enemyPosAverage = Vector3.zero;
        foreach (Target target in targets)
        {
            enemyPosAverage += target.position;
        }
        enemyPosAverage /= targets.Count; // beware of divide by zero if targets is initialized empty.
        enemyPosAverage.y = 0;

        // get the transform.positon in 2D
        Vector3 tempTransform = transform.position;
        tempTransform.y = 0;

        // Calculate difference between rotation to target and current rotation.
        Vector3 lookDirection = (enemyPosAverage - tempTransform).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        if (turnRate <= float.Epsilon || Quaternion.Angle(transform.rotation, lookRotation) < firingCone)
            return true;

        else // rotate to target
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnRate);
            return false;
        }



    }
    protected abstract bool PrioritiseTargets(Target targetInRange, Target storedTarget);
}

/// <summary>
/// Priority Enemies closest to the tower.
/// </summary>
public class CloseTargeter : EnemyTargeter
{
    protected override bool PrioritiseTargets(Target targetInRange, Target storedTarget)
    {

        // if target in range distance is less swap
        return (Vector3.Distance(targetInRange.position, transform.position) < Vector3.Distance(storedTarget.position, transform.position));
    }
}
/// <summary>
/// Priority enemy with the most surrounding enemies in a radius around them.
/// </summary>
public class ClusterTargeter : EnemyTargeter
{
    protected override bool PrioritiseTargets(Target targetInRange, Target storedTarget)
    {
        //Debug.LogWarning("this Targeter cannot be implemented efficiently without the neighbourhood of flocking behaviour");
        // if neighboorhoud is bigger swap
        // return (targetInRange.enemy.neighbourhood.count > storedTarget.enemy.neighbourhood.count);
        // for now generate the neighbour hood myself using a layer mask and overlap sphere which has been known to end badly
        return (Physics.OverlapSphere(targetInRange.position, 1.5f, enemyLayer).Length > Physics.OverlapSphere(storedTarget.position, 1.5f, enemyLayer).Length);
    }
}
/// <summary>
/// Priority enemies with higher speed.
/// </summary>
public class FastTargeter : EnemyTargeter
{
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
    protected override bool PrioritiseTargets(Target targetInRange, Target storedTarget)
    {
        return (targetInRange.enemy.MaxHealth > storedTarget.enemy.MaxHealth);
    }
}