using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TrapDetails
{
    public float inkLevel;
    public float dps;
    public float startupTime;
    public Condition[] conditions;

    public TrapDetails()
    {
        inkLevel = 0;
        dps = 0;
        startupTime = 0;
        conditions = new Condition[0];
    }

    public TrapDetails(float inkLevelInit, float dpsInit, float startupTimeInit,
        Condition[] conditionsInit)
    {
        inkLevel = inkLevelInit;
        dps = dpsInit;
        startupTime = startupTimeInit;
        conditions = conditionsInit;
    }
}

public class TrapAttacker : Attacker
{
    private List<Target> targets;
    private readonly List<GameObject> placedTraps = new();
    [SerializeField] GameObject trapPrefab;

    [SerializeField] private int maxTrapCount;
    [SerializeField] private float bufferDistance = 1f;

    public TrapDetails inkDetails = new();
    private int inkPlacementIndex;
    private LayerMask obstacles = 0;

    public override void Attack(HashSet<Target> newTargets)
    {
        if (!attacking)
        {
            AnimateAttack();

            if (attackParticlePrefab != null)
            {
                GameObject particle = Object.Instantiate(attackParticlePrefab, transform);
                particle.transform.position += new Vector3(0, particleOriginOffset, 0);
                Object.Destroy(particle, 1.5f);
            }

            Debug.Log("Trap Attacker");

            TrapManager.trapAttackers.Add(this);

            targets = newTargets.ToList();
            attacking = true;
        }

        if (!CheckCooldownTimer()) return;

        CleanUp();
        delayTimer = 0f;
        cooldownTimer = 0f;
        attacking = false;
    }

    public bool PlaceTrap()
    {
        if (obstacles == 0) obstacles = LayerMask.GetMask("Trap", "Tower", "Pylon", "Hub"); 

        if (placedTraps.Count >= maxTrapCount || inkPlacementIndex >= targets.Count)
        {
            inkPlacementIndex = 0;
            TrapManager.trapAttackers.Remove(this);
            return true;
        }

        if (inkPlacementIndex >= targets.Count)
        {
            inkPlacementIndex = 0;
            TrapManager.trapAttackers.Remove(this);
            return false;
        }

        if (Physics.OverlapSphere(targets[inkPlacementIndex].position, bufferDistance, obstacles).Length > 0)
        {
            inkPlacementIndex++;
            return PlaceTrap();
        }

        GameObject newTrap = Object.Instantiate(trapPrefab, targets[inkPlacementIndex].position, Quaternion.identity);
        TrapAttackObject trapScript = newTrap.GetComponent<TrapAttackObject>();

        trapScript.details.inkLevel = inkDetails.inkLevel;
        trapScript.details.dps = damage;
        trapScript.details.startupTime = attackDelay;
        trapScript.details.conditions = inkDetails.conditions;

        placedTraps.Add(newTrap);
        inkPlacementIndex++;
        return true;
    }

    void CleanUp()
    {
        foreach (GameObject trap in placedTraps)
        {
            Object.Destroy(trap);
        }
        placedTraps.Clear();
    }
}