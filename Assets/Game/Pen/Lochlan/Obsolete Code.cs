#region Turret Controller
/*
using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Functionality moved to Tower", true)]
public class TurretController : MonoBehaviour
{
    
 // TODO: test if unity adds enemies colliding with it when it is instantiated to the inRangeEnemies or if I need to add them via sphere cast on start.

    //Enemy catalouging
    private HashSet<GameObject> inRangeEnemies = new();
    public GameObject targetGameObject; // change this to private when bullet script is no longer required.
    private Enemy targetEnemy;

    // bad firing animation
    public GameObject bullet;
    Vector3 bulletSpawn1;
    Vector3 bulletSpawn2;
    bool barrelAlternate;

    // pylon data
    public bool towerActive = true;
    public int storedExperience;

    // tower values
    public float damage = 100;
    public float firingInterval = 3;
    private float firingClock = 2;
    public float turnSpeed = 2;
    public float firingCone = 20;
    public bool lockedOn = false;

    void Start()
    {
        this.enabled = false;

        bulletSpawn1 = transform.GetChild(1).transform.localToWorldMatrix.GetPosition();
        bulletSpawn2 = transform.GetChild(2).transform.localToWorldMatrix.GetPosition();

       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!towerActive)
            return;

        firingClock += Time.fixedDeltaTime;
        if (targetEnemy)
        {
            // rotate turret to targetted enemy
            RotateToTarget();

            if (targetEnemy.Dead)
            {
                // take enemy experience
                storedExperience += targetEnemy.expValue;
                targetEnemy.expValue = 0;
                
                // remove it from targets and retarget
                inRangeEnemies.Remove(targetGameObject);
                PickPriorityTarget();
            }

            if (firingClock > firingInterval && lockedOn)
                Attack();
        }    
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!other.gameObject.GetComponent<Enemy>().Dead)
                inRangeEnemies.Add(other.gameObject);
        }

        if (targetEnemy is null) PickPriorityTarget();
    }

    private void OnTriggerExit(Collider other)
    {
        inRangeEnemies.Remove(other.gameObject);
        if (targetGameObject == other.gameObject) // immediately remove object from the target if it is the current target.
        {
            targetGameObject = null;
            targetEnemy = null;
        }
        PickPriorityTarget();
    }

    private void Attack() // this should be overridden in child classes
    {
        // do attack animation


        GameObject bulletRef;
        if (barrelAlternate)
        {
            bulletRef = Instantiate(bullet, bulletSpawn1, Quaternion.identity);
        }
        else
        {
            bulletRef = Instantiate(bullet, bulletSpawn2, Quaternion.identity);
        }
        barrelAlternate = !barrelAlternate;

        //bulletRef.GetComponent<Bullet>().target = targetGameObject;

        //targetEnemy.TakeDamage((int)damage);

        firingClock = 0;


    }


    public void PickPriorityTarget()
    {
        lockedOn = false;
        if (inRangeEnemies.Count <= 0)
        {
            targetGameObject = null;
            targetEnemy = null;
            return;
        }
        int bestScoreSoFar = -1;
        GameObject bestTargetSoFar = new(); // I don't know why I have to assign this something but it doesn't work otherwise
        GameObject deleteThis = bestTargetSoFar; // so feel free to roll over this if you know how to do better.

        foreach (GameObject thisEnemy in inRangeEnemies)
        {          
            int thisScore = TargetingAlgorithm(thisEnemy);
            if (thisScore > bestScoreSoFar)
            {
                bestScoreSoFar = thisScore;
                bestTargetSoFar = thisEnemy;
            }
        }
        Destroy(deleteThis);
        targetGameObject = bestTargetSoFar;
        targetEnemy = bestTargetSoFar.GetComponent<Enemy>();
    }

    int TargetingAlgorithm(GameObject enemy)  // this should be overridden in child classes
    {
        
        return UnityEngine.Random.Range(0, 10);
    }

    void RotateToTarget()  // this should be overridden in child classes
    {
        Vector3 lookDirection = (targetGameObject.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

        if (!lockedOn && Quaternion.Angle(transform.rotation, lookRotation) < firingCone)
            lockedOn = true;

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }
}

 */
#endregion

#region LockOnTag
/*
  [Obsolete]private List<LockOnTarget> lockOnTargets = new List<LockOnTarget>();

 [Obsolete("LockOnTag is deprecated, please use LockedOn instead.")]
    /// <summary>
    /// Here's the thing this works as far as maintaining locks on the targets in range with the highest max health but in the case a better target enters 
    /// it's range it will immediately stop and try locking onto the new better target. unfortunately the only way to prevent this would be to forcefully maintain
    /// a lock until a target goes out of range
    /// </summary>
    private void LockOnTag()
    {

         PsuedoCode
         
        new local hashset of Target marked = targets deep copy           // copy the references stored in targets but not the reference to targets

        new local hashset of Target targetsLockedFire 
        
        for each targetLock in lockOnTargets
            if targetLlock is in targets
                update the timer
                remove that enemy to the marked targets hash
                if targetLocked is true
                    add it to the targetsLockedFire set
                else
                    if the timer on lockTarget has expired
                        add it to the targetsLockedFire set
                        reset timer on lockTarget
                        if continuous
                            targetLocked is true
            else targetLock is not in targets so
                remove it from lockOnTargets

        for each target in marked
            add it to lockOnTargets

        call attack on targetsLockedFire

         

using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using System;
using UnityEngine;

HashSet<Target> marked = new HashSet<Target>(targets);

HashSet<Target> targetsLockedFire = new HashSet<Target>(); // to handle the attack call

for (int i = 0; i < lockOnTargets.Count; i++)
{

if (targets.Contains(lockOnTargets[i].target))
{
lockOnTargets[i].IncrementLockTimer();
        // progress lock on animation
marked.Remove(lockOnTargets[i].target);
if (lockOnTargets[i].targetLocked)
{
Debug.DrawLine(transform.position, lockOnTargets[i].target.position, Color.red, 0.02f);
targetsLockedFire.Add(lockOnTargets[i].target);
}
else
{
if (lockOnTargets[i].lockOnProgress > lockOnDuration)
{
targetsLockedFire.Add(lockOnTargets[i].target);
lockOnTargets[i].lockOnProgress = 0;
//if (continuous) lockOnTargets[i].targetLocked = true;
}
}
}
else lockOnTargets.Remove(lockOnTargets[i]);     // this won't work because it modifies the list the loop is dependent on         
}

foreach (Target target in marked)
{
lockOnTargets.Add(new LockOnTarget(target));
}

if (targetsLockedFire.Count > 0) attackerComponent.Attack(targetsLockedFire);

    }

    [Obsolete("only used in the obsolete method LockOnTag")]
// I hate that this is neccesary
class LockOnTarget
{
    public Target target;
    public bool targetLocked;
    public float lockOnProgress;

    public LockOnTarget(Target inputTarget)
    {
        target = inputTarget;
        targetLocked = false;
        lockOnProgress = 0;
    }

    public void IncrementLockTimer() { lockOnProgress += Time.deltaTime; }
}

*/
#endregion

#region Accelerate
/*
 //Accelerate
    private bool accelerate = false;
    public bool accelerated = false; // determines if a tower is currently accelerated
    readonly float accelTimeMax = 5; // the time a tower will go without killing before accelerate resets
    public float accelTimer = 0; // timer to keep track of the above.
    public readonly float accelSpeedMod = 0.2f; // on kill multiply the attack delay by this basically increase by 50%
    private float accelModReverse;
    public bool GetAccelerate() => accelerate; // determines if a tower can accelerate

private void Start()
{
    accelModReverse = 1 / accelSpeedMod;
}

private void Update
{
                AccelerateTag();
}

    public void AccelerateTag()
    {
        if (accelerated)
        {
            accelTimer += Time.deltaTime;
            if (accelTimer > accelTimeMax)
            {
                accelerated = false;
                attackerComponent.attackDelay *= accelModReverse;// return attack delay to normal
                accelTimer = 0; // Reset timer
            }
        }
    }

void HandleTargetEnemyDeath()
    {
        if (target.enemy.CheckIfDead())
        {
            // extract exp
            originTower.storedExperience += target.enemy.expValue;
            target.enemy.expValue = 0;

            if (originTower.GetAccelerate()) // Accelerate logic
            {
                if (!target.enemy.Dead) // The bool Dead is set in OnDeath() so if it is false we can be sure this attack dealt the killing blow as the enemy has no health but hasn't "died" yet.
                {
                    originTower.accelerated = true; // this could be called from elsewhere if neccesary
                    originTower.accelTimer = 0;
                    originTower.AttackerComponent.attackDelay *= originTower.accelSpeedMod;// modify attack delay
                }
            }
            target.enemy.OnDeath(); // enemy on death
        }
    }
 */
#endregion

#region MultiTarget
/*
 //Multitarget
    private bool multiTarget = false; // if true tower will have multiple targets otherwise defaults to 1
    private int numTargets; // number of targets if multiTarget is true.
    // private int targetProjectileSpeedCounter; // the current index of targets to be assigned a projectile speed during multitarget.

private void Start()
{

        if (multiTarget) if (numTargets <= 0) Debug.LogWarning("variable numTargets has not been assigned this tower will search for 0 targets.");

}

private void Update()
{
    //if (multiTarget)
    //  targets = targeterComponent.AcquireTargets(numTargets); // Multi-Target &*
    //else targets = targeterComponent.AcquireTargets(); // &*
}

*/
#endregion

#region Quit Script
/*
 [Obsolete("Functionality moved to OpenPause UI", true)]
public class QuitScript : MonoBehaviour
{
    Button button;

    private void Start()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(Quit);
    }
    
    // get rid of this script when you have an actual dedicated quit or pause UI manager.
    private void Quit()
    {
        Debug.Log("Quit button has ben pressed but no unfortunately you can't quit unity.");
        Debug.Log("I meant the editor...");
        Application.Quit();
    }
}
 */
#endregion






