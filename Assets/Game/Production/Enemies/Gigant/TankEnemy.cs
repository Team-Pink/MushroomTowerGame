using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemy : Enemy
{    
    void Start()
    {
        
    }

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
