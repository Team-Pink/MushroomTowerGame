using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

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

    public TrapDetails inkDetails;
    private int inkPlacementIndex;
    private LayerMask obstacles = 0;

    public override void Attack(HashSet<Target> newTargets)
    {
        if (onCooldown)
        {
            if (cooldownTimer >= attackCooldown)
            {
                onCooldown = false;
                cooldownTimer = 0;
                delayTimer = 0;
            }
            else
            {
                cooldownTimer += Time.deltaTime;
                return;
            }
        }

        if (!attacking)
        {
            AnimateAttack();

            placedTraps.Clear();

            targets = newTargets.ToList();
            attacking = true;
        }
    }

    public bool PlaceTrap()
    {
        inkDetails = trapPrefab.GetComponent<TrapAttackObject>().details;

        if (obstacles == 0) obstacles = LayerMask.GetMask("Trap", "Shroom", "Node", "Meteor"); 

        if (placedTraps.Count >= maxTrapCount || inkPlacementIndex >= targets.Count)
        {
            inkPlacementIndex = 0;
            TrapManager.trapAttackers.Remove(this);
            attacking = false;
            onCooldown = true;
            return true;
        }

        if (inkPlacementIndex >= targets.Count)
        {
            inkPlacementIndex = 0;
            TrapManager.trapAttackers.Remove(this);
            attacking = false;
            onCooldown = true;
            return false;
        }

        if (Physics.OverlapSphere(targets[inkPlacementIndex].getPosition(), bufferDistance, obstacles).Length > 0)
        {
            inkPlacementIndex++;
            return PlaceTrap();
        }

        GameObject newTrap = UnityObject.Instantiate(trapPrefab, targets[inkPlacementIndex].getPosition(), Quaternion.Euler(0, Random.Range(0, 360), 0));
        TrapAttackObject trapScript = newTrap.GetComponent<TrapAttackObject>();

        trapScript.cleanupDuration = attackCooldown;
        trapScript.details.inkLevel = inkDetails.inkLevel;
        trapScript.details.dps = damage;
        trapScript.details.startupTime = attackDelay;
        trapScript.details.conditions = inkDetails.conditions;

        placedTraps.Add(newTrap);
        inkPlacementIndex++;
        return true;
    }

    public override void AnimateProjectile()
    {
        if (attackSoundEffect != null)
        {
            AudioManager.PlaySoundEffect(attackSoundEffect.name, 1);
        }

        if (attackParticlePrefab != null)
        {
            GameObject particle = UnityObject.Instantiate(attackParticlePrefab, transform);
            particle.transform.position += new Vector3(0, particleOriginOffset, 0);
            UnityObject.Destroy(particle, 0.5f);
        }

        Debug.Log("Ink sprayed " + attackDelay);

        TrapManager.trapAttackers.Add(this);
    }
}