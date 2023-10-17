using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Target
{
    public Vector3 position; // expands in practice to enemy.transform.position if an enemy exists.
    public Enemy enemy;
    float timeFound
    {
        get => Time.time;
    }

    public Target(Vector3 targetPos, Enemy targetEnemy = null)
    {
        position = targetPos;
        enemy = targetEnemy;
    }
}

[Serializable]
public enum TargeterType
{
    SelectAType,
    Close,
    Cluster,
    Fast,
    Strong,
    Track
} // For Editor Use Only
[Serializable]
public enum AttackerType
{
    SelectAType,
    Area,
    Single,
    Trap
} // For Editor Use Only

[Serializable]
public struct Details
{
    public string name;
    public TargeterType targeterType;
    public AttackerType attackerType;

    public Details(string nameInit, TargeterType targeterInit, AttackerType attackerInit)
    {
        name = nameInit;
        targeterType = targeterInit;
        attackerType = attackerInit;
    }
} // For Editor Use Only

public class Tower : Building
{
    // Startup
    [SerializeField] float growthDuration = 5.0f;
    private float growthTime = 0.0f;
    private bool recovering = false;
    [SerializeField] float recoveryDuration = 2.0f;
    private float recoveryTime = 0.0f;


    // Components
    [SerializeField] protected Animator animator;
    [SerializeReference] private Attacker attackerComponent;
    [SerializeReference] private Targeter targeterComponent;
    public Details details; // For Editor Use Only
    protected new Transform transform;

    public Attacker AttackerComponent { get => attackerComponent; set => attackerComponent = value; }

    public Targeter TargeterComponent { get => targeterComponent; set => targeterComponent = value; }

    // References
    private HashSet<Target> targets = new();


    // Pylon Data
    public int storedExperience;

    // Upgrading
    [SerializeField] bool upgradeable;
    public bool Upgradeable { get; private set; }
    private GameObject upgradePrefabL;
    public int upgradeCostL;
    private GameObject upgradePrefabR;
    public int upgradeCostR;

    // Purchasing
    public int purchaseCost = 10;
    [Range(0, 1)] public float sellReturnPercent = 0.5f;


    // prefabs
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject attackObjectPrefab;

    // Tower values
    [SerializeField] private float projectileSpeed = 1.5f; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! assign this properly on prefabs!
    // setting the above to zero means time to target will equal zero because (Distance * projectileSpeed). Doing this will cause attacks to occur the frame their cooldown ends and they have a target.

    // Tags from Lochlan


    //Multitarget
    private bool multiTarget = false; // if true tower will have multiple targets otherwise defaults to 1
    private int numTargets; // number of targets if multiTarget is true.
    // private int targetProjectileSpeedCounter; // the current index of targets to be assigned a projectile speed during multitarget.

    //Accelerate
    private bool accelerate = false;
    public bool accelerated = false; // determines if a tower is currently accelerated
    readonly float accelTimeMax = 5; // the time a tower will go without killing before accelerate resets
    public float accelTimer = 0; // timer to keep track of the above.
    public readonly float accelSpeedMod = 0.2f; // on kill multiply the attack delay by this basically increase by 50%
    private float accelModReverse;
    public bool GetAccelerate() => accelerate; // determines if a tower can accelerate

    //Lock On
    private bool lockOn = false; // determines if the tower will lock on to an enemy.
    private float lockOnDuration = 1.5f;
    private List<LockOnTarget> lockOnTargets = new List<LockOnTarget>();

    //Continuous
    [SerializeField] private bool continuous = false;

    [SerializeField] AudioClip buildAudio;

    private void Awake()
    {
        attackerComponent.animator = animator;

        transform = gameObject.transform;
        targeterComponent.transform = transform;
        attackerComponent.transform = transform;

        targeterComponent.enemyLayer = LayerMask.GetMask("Enemy");

        attackerComponent.bulletPrefab = bulletPrefab;
        AttackerComponent.attackObjectPrefab = attackObjectPrefab;
        attackerComponent.originReference = this; // I am very open to a better way of doing this so please if you can rearchitect this go ahead. !!!

        radiusDisplay.transform.localScale = new Vector3(2 * targeterComponent.range, 2 * targeterComponent.range);

        accelModReverse = 1 / accelSpeedMod;
        if (multiTarget) if (numTargets <= 0) Debug.LogWarning("variable numTargets has not been assigned this tower will search for 0 targets.");

        AudioManager.PlaySoundEffect(buildAudio.name, 1);
    }

    private void Update()
    {
        if (growthTime < growthDuration)
        {
            growthTime += Time.deltaTime;
            return;
        }

        if (recovering)
        {
            if (recoveryTime < recoveryDuration)
            {
                recoveryTime += Time.deltaTime;
                return;
            }
            else
            {
                recovering = false;
                base.Reactivate();
            }
        }

        if (Active)
        {
            if (multiTarget)
                targets = targeterComponent.AcquireTargets(numTargets); // Multi-Target &*
            else targets = targeterComponent.AcquireTargets(); // &*
            if (targets != null)
            {
                if (lockOn) // this is terrible code
                {
                    LockOnTag();
                }
                else if (attackerComponent.bounce)
                    {
                        if (attackerComponent.bounceBulletTowersPossession)
                        {
                            attackerComponent.Attack(targets); // Generates an attack query that will create an attack object.
                        }
                            
                    }                    
                else if (attackerComponent.CheckCooldownTimer())
                {
                    CalcTimeToTarget(targets, transform.position);
                    attackerComponent.Attack(targets);
                }
                    




                // Attack tags
                AccelerateTag();

            }
        }
    }



    public void Upgrade(int upgradePath)
    {
        if (upgradePath == 0 || upgradePath == 1)
        {
            GameObject selectedUpgradePrefab;

            if (upgradePath == 1)
                selectedUpgradePrefab = upgradePrefabL;
            else
                selectedUpgradePrefab = upgradePrefabR;

            Instantiate(selectedUpgradePrefab, transform.position, transform.rotation, transform.parent);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Upgrade only accepts an int value of 0 or 1", this);
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        animator.SetTrigger("Deactivate");
        recoveryTime = 0.0f;
    }

    public override void Reactivate()
    {
        recovering = true;
        animator.SetTrigger("Reactivate");
    }

    public override void Sell()
    {
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(purchaseCost, sellReturnPercent);

        Destroy(gameObject);

        base.Sell();
    }

    public override int GetTowerEXP()
    {
        int tempExp = storedExperience;
        storedExperience = 0;
        return tempExp;
    }

    /// <summary>
    /// literally exists because bounce needs to port it's own stuff for 
    /// </summary>
    /// <returns></returns>
    public GameObject GetAttackObjectPrefab()
    {
        return attackObjectPrefab;
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

    #region LockOn

    /// <summary>
    /// Here's the thing this works as far as maintaining locks on the targets in range with the highest max health but in the case a better target enters 
    /// it's range it will immediately stop and try locking onto the new better target. unfortunately the only way to prevent this would be to forcefully maintain
    /// a lock until a target goes out of range
    /// </summary>
    private void LockOnTag()
    {

        /* PsuedoCode
         
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

         */

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
                        if (continuous) lockOnTargets[i].targetLocked = true;
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

    #endregion

    /// <summary>
    /// uses distance to target and attackSpeed to calculate the travel time of an attack to it's target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="originPos"></param>
    public void CalcTimeToTarget(HashSet<Target> targets, Vector3 originPos)
    {
        //int TargetCounter = 0;
        foreach (Target target in targets)
        {
            
            attackerComponent.attackDelay = Vector3.Distance(originPos, target.position) * projectileSpeed;
            return;
            /*TargetCounter++;
            if (TargetCounter == targetProjectileSpeedCounter)
            {
                if (targetProjectileSpeedCounter >= numTargets)
                    targetProjectileSpeedCounter = 0;
                return; // yes I am indeed entering a foreach loop just to get a reference to the first object in targets and then returning without examining the other targets.
            }*/
            
        }

    }
}
