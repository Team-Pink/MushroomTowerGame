using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleAttacker : Attacker
{
    public override void Attack(HashSet<Target> targets)
    {
        base.Attack(targets);

        if (delayTimer < 1 && cooldownTimer < 1) return;

        foreach (var target in targets)
            target.enemy.health -= damage;

        delayTimer = 0;
        cooldownTimer = 0;
    }
}
