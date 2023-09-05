using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrapDetails
{
    public float inkLevel;
    public int damage;
    public float damageRate;
    public bool poisonous;
    public bool sticky;

    public TrapDetails()
    {
        inkLevel = 0;
        damage = 0;
        damageRate = 0;
        poisonous = false;
        sticky = false;
    }

    public TrapDetails(float inkLevelInit,
        int damageInit, float damageRateInit,
        bool poisonousInit = false, bool stickyInit = false)
    {
        inkLevel = inkLevelInit;
        damage = damageInit;
        damageRate = damageRateInit;
        poisonous = poisonousInit;
        sticky = stickyInit;
    }
}

// From Lochlan: I don't think there should be a trap attacker I think enemies should deal with traps in their own scripts based
// on the state of the Flow field tile they are standing on.
public class TrapAttacker : Attacker 
{
    private HashSet<Transform> trapPositions; // make this vector 3 for the tech implementation and make a new set for the prefabs to clean up
    [SerializeField] GameObject trapPrefab;

    private int maxTrapCount;
    private float bufferDistance = 0.5f;

    //dunno where to put this
    float trapTickSpeed = 1f;

    public TrapDetails inkDetails = new();

    public override void Attack(HashSet<Target> targets)
    {
        //Play attack animation here

        if (!CheckDelayTimer()) return;

        if (cooldownTimer == 0f)
        {
            Debug.Log("Trap Attacker");
            
            foreach (Target targetPos in targets)
            {
                if (trapPositions.Count >= maxTrapCount) break;  //Changed this from 'continue' -Finn

                bool failedCheck = false;
                foreach (Transform trap in trapPositions)
                {
                    if ((targetPos.position - trap.position).sqrMagnitude < bufferDistance * bufferDistance)    //If we find a trap in range of this attempted placement...
                    {
                        failedCheck = true; //Abandon the placement
                        break;
                    }
                }
                if (failedCheck) continue;

                Trap spawnedTrap = Object.Instantiate(trapPrefab, targetPos.position, Quaternion.identity).AddComponent<Trap>();
                spawnedTrap.Construct(damage, trapTickSpeed);

                trapPositions.Add(spawnedTrap.transform);
            }
        }

        if (!CheckCooldownTimer()) return;
        CleanUp();
        delayTimer = 0f;
        cooldownTimer = 0f;
    }

    void CleanUp()
    {
        foreach (Transform trap in trapPositions)
        {
            Object.Destroy(trap);
        }
        trapPositions.Clear();
    }
}

//dud class delete when Trap class Exists
public class Trap : MonoBehaviour
{
    int damage = 0;
    float attackDelay = 0f;

    public void Construct(int damageToSet, float tickDelayToSet)
    {
        damage = damageToSet;
        attackDelay = tickDelayToSet;
    }
}