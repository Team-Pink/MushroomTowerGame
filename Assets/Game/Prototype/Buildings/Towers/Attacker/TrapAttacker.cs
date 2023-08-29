using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TrapAttacker : Attacker
{
    HashSet<Transform> traps;
    [SerializeField] GameObject trapPrefab;

    int placedTraps;
    float bufferDistance = 0.5f;

    //dunno where to put this
    float trapTickSpeed = 1f;


    //handles trap placement not trap behaviour
    public override void Attack(HashSet<Target> targets)
    {
        //Play attack animation here

        if (!CheckDelayTimer()) return;

        if (cooldownTimer == 0f)
        {
            Debug.Log("Trap Attacker");
            
            foreach (var targetPos in targets)
            {
                if (traps.Count == placedTraps) continue;

                bool failedCheck = false;
                foreach (var trap in traps)
                {
                    //this is the Euclidean Distance Equation... used for getting the distance between 2 points
                    float distance = Mathf.Sqrt(Mathf.Pow(targetPos.position.x - trap.position.x, 2) + Mathf.Pow(targetPos.position.z - trap.position.z, 2));
                    if (distance < bufferDistance) failedCheck = true;
                }
                if (failedCheck) continue;

                Trap spawnedTrap = Object.Instantiate(trapPrefab, targetPos.position, Quaternion.identity).AddComponent<Trap>();
                spawnedTrap.Construct(damage, trapTickSpeed);

                traps.Add(spawnedTrap.transform);
            }
        }

        if (!CheckCooldownTimer()) return;
        CleanUp();
        delayTimer = 0f;
        cooldownTimer = 0f;
    }

    void CleanUp()
    {
        foreach (var trap in traps)
        {
            Object.Destroy(trap);
        }
        traps.Clear();
        //Ratilda was here
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