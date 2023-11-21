using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An object responsible for carrying out the effects of an attack on an enemy from the time after an attack is launched to when it resolves.
/// </summary>
public class AttackObject : MonoBehaviour
{
    public int damage; // damage dealt to target
    public float delayToTarget; // time until the attack reaches the target
    public Target target; // targets of the attack
    public Shroom originShroom; // origin of the attack
    public GameObject hitParticlePrefab;
    public AudioClip hitSoundEffect;
    public float damageRadius;
    public LayerMask mask;

    public int tagSpecificDamage;
    public HashSet<Enemy> tagSpecificEnemiesHit = new(); //enemies that were hit as a result of tags like spray
}