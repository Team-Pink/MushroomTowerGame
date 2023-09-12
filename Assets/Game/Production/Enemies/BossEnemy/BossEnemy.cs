using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    
    void Update()
    {
        if (!Dead)
            Playing();
    }

    protected override void Playing()
    {
        base.Playing();
    }
}
