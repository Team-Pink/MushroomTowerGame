using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapAttacker : Attacker
{
    HashSet<Transform> traps;

    public override void Attack(HashSet<Target> targets)
    {
        base.Attack(targets);

        if (delayTimer < 1 && cooldownTimer < 1) return;

        CleanUp();

        //Create, construct (constructor), and cache traps at target positions
    }

    void CleanUp()
    {
        foreach (var trap in traps)
        {
            MonoBehaviour.Destroy(trap);
        }
        traps.Clear();
    }
}

