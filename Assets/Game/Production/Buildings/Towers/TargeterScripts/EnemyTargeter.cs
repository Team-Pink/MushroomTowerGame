using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class EnemyTargeter : Targeter
{
    Quaternion defaultRotation = Quaternion.identity;
    HashSet<Target> bestTargets = new();
    public float firingCone = 20;

    public void GetTargetsInRange()
    {
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, range, enemyLayer);
        if (enemyColliders == null) return;
        
        
      /*
        HashSet<Enemy> enemySet = new HashSet<Enemy>(); // the Set of enemies to add into the inRangeEnemiesList
        HashSet<Target> targetsToRemove = new HashSet<Target>(); // the set of enemies to remove from the inRangeEnemies List
        foreach (Collider collider in enemyColliders)
        {
            enemySet.Add(collider.GetComponent<Enemy>());
        }
        
        foreach(Target target in targetsInRange)
        {

            if (enemySet.Contains(target.enemy))
            {
                
                enemySet.Remove(target.enemy); // we have that enemy already so remove it.          
               
            }
            else targetsToRemove.Add(target); // we don't have the enemy anymore so prepare to remove it.
        }

        foreach (Target oldTarget in targetsToRemove)
        {
            targetsInRange.Remove(oldTarget);
        }
        foreach(Enemy newTarget in enemySet)
        {
            targetsInRange.Add(new Target(newTarget.transform.position, newTarget));
        }

        //this is pointless it doesn't solve the problem
    
    */

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

        GetTargetsInRange(); // update targets in range
        if (targetsInRange.Count == 0 || targetsInRange == null) // null check out.
        {
            if (transform.rotation != defaultRotation) // ???
                transform.rotation = defaultRotation; // ??? If there is no target instantly return to facing the camera?
            return null;
        }
        if (targetsInRange.Count <= numTargets) // early out if less targets than numTargets.
        {
            bestTargets.Clear(); // discard best targets
            foreach (Target target in targetsInRange)
            {
                bestTargets.Add(target); // add any targets you can.
            }
            if (CheckRotation(bestTargets)) // rotate towards those targets
                return bestTargets;
        }

        foreach (Target target in targetsInRange)
        {
            if (bestTargets.Count < numTargets) // if best targets is empty or not full add until it is full.
                bestTargets.Add(target);
            else
            {
                Target toRemove = target; // the value that will hold the lowest priority target
                bool swap = false; // if target is not the lowest priority then swap it with the lowest priority.
                foreach (Target storedTarget in bestTargets)
                {
                    if (PrioritiseTargets(toRemove, storedTarget)) // check best fit.
                    {
                        toRemove = storedTarget; // update the lowest priority.
                        swap = true;
                    }
                }

                if (swap)
                {
                    bestTargets.Remove(toRemove); //remove lowest priority
                    bestTargets.Add(target); // add replacement Target
                }
            }
        }
        //foreach (Target target in bestTargets) // for testing
        //{
        //    Debug.DrawLine(transform.position, target.position, Color.red, 0.02f);
        //}
        if (CheckRotation(bestTargets)) // if targets not in firing cone return false, rotate towards targets.
            return bestTargets;

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
        enemyPosAverage /= targets.Count; // beware of divide by zero if BestTargets is initialized empty.
        enemyPosAverage.y = 0; // don't care about the y plane.

        // get the transform.positon in 2D
        Vector3 tempTransform = transform.position; // transform.position is a value type so don't worry about the line below it won't modify the actual position.
        tempTransform.y = 0;// don't care about the y plane.

        // Calculate difference between rotation to target and current rotation.
        Vector3 lookDirection = (enemyPosAverage - tempTransform).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnRate); // rotate to target
        if (turnRate <= float.Epsilon || Quaternion.Angle(transform.rotation, lookRotation) < firingCone) // setting the turn rate of a tower to 0 means it doesn't nedd to turn at all.
            return true;
        else
        { 
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