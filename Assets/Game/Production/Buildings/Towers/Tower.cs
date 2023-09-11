using System;
using System.Collections.Generic;
using UnityEngine;

public struct Target
{
    public Vector3 position;
    public Enemy enemy;

    public Target(Vector3 targetPos, Enemy targetEnemy = null)
    {
        position = targetPos;
        enemy = targetEnemy;
    }
}

[Serializable]
public enum TargeterType
{
    Close,
    Cluster,
    Fast,
    Strong,
    Track
} // For Editor Use Only
[Serializable]
public enum AttackerType
{
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
    // Components
    protected new Transform transform;
    protected Animator animator;
    [SerializeReference] private Attacker attackerComponent;
    [SerializeReference] private Targeter targeterComponent;
    public Details details; // For Editor Use Only



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

    // Tags from Lochlan


    //Multitarget
    private bool multiTarget = false; // if true tower will have multiple targets otherwise defaults to 1
    private int numTargets; // number of targets if the above is true.

    //Accelerate
    private bool accelerate = false;
    [SerializeField] public bool accelerated = false; // determines if a tower is currently accelerated
    readonly float accelTimeMax = 5; // the time a tower will go without killing before accelerate resets
    public float accelTimer = 0; // timer to keep track of the above.
    public readonly float accelSpeedMod = 0.5f; // on kill multiply the attack delay by this basically increase by 50%
    public readonly float decreaseAccel = 1.25f; //acceleration decreases by 25% if the tower fails a kill.
    private float baseDelay; // the original starting delay of the tower
    public bool GetAccelerate() => accelerate; // determines if a tower can accelerate

    //Lock On
    private bool lockOn = false; // determines if the tower will lock on to an enemy.
    private float lockOnDuration = 1.5f;
    private List<LockOnTarget> lockOnTargets;

    //Continuous
    private bool continuous;
    private bool targetLocked;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        transform = gameObject.transform;
        targeterComponent.transform = transform;
        attackerComponent.transform = transform;

        targeterComponent.enemyLayer = LayerMask.GetMask("Enemy");

        attackerComponent.bulletPrefab = bulletPrefab;
        AttackerComponent.attackObjectPrefab = attackObjectPrefab;
        attackerComponent.originReference = this; // I am very open to a better way of doing this so please if you can rearchitect this go ahead.

        radiusDisplay.transform.localScale = new Vector3(2 * targeterComponent.range, 2 * targeterComponent.range);

        if (accelerate) baseDelay = attackerComponent.attackDelay;
    }

    private void Update()
    {
        if (Active)
        {
            if (multiTarget) targets = targeterComponent.AcquireTargets(numTargets); // Multi-Target &*
            else targets = targeterComponent.AcquireTargets(); // &*
            if (targets != null)
            {
                if (lockOn) // this is terrible code
                {
                    LockOnTag();
                }
                else
                    attackerComponent.Attack(targets); // Generates an attack query that will create an attack object.



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

    public void AccelerateTag()
    {
        if (accelerated)
        {
            accelTimer += Time.deltaTime;
            if (accelTimer > accelTimeMax || AttackerComponent.attackDelay > baseDelay)
            {
                accelerated = false;
                attackerComponent.attackDelay = baseDelay;// return attack delay to normal
                accelTimer = 0; // Reset timer
            }
        }
    }

    /// <summary>
    /// Personally I think this is a bad tag, I could understand if the Idea was to have the attacker play a lock on animation that uses the attackDelay as the lockOnTime
    /// but to have the tower hold off on attacking and 
    /// </summary>
    private void LockOnTag()
    {

        /* Lock on loop PsuedoCode
         
        new local hashset of marked targets 
        marked = targets
        
        for each targetLock in lockOnTargets
            check if targetLlock is in targets
            if it's in there, fix the timer, and remove that enemy to the marked targets hash
            if it isn't in there, remove it
            

        for each target in marked
            add it to lockOnTargets

         */

        HashSet<Target> marked = new HashSet<Target>;   //Right now this is just creating another reference to the same thing
        marked = targets;

        foreach (LockOnTarget targetLock in lockOnTargets)
        {

                if (targets.Contains(targetLock.target))
            {
                targetLock.IncrementLockTimer(); 
            }
                    lockOnTargets.Remove(targetLock);
                else
                    

                if(targetLock.target.enemy == target.enemy)addTarget = false;

        }




        //if (lockOnTargets != targets)
        {
            lockOnTargets = targets;
            lockOnProgress = 0;
            targetLocked = false;
        }
        else if (targetLocked)
        {
            attackerComponent.Attack(lockOnTargets);
        }
        else
        {
            lockOnProgress += Time.deltaTime;
            if (lockOnProgress > lockOnDuration)
            {
                attackerComponent.Attack(lockOnTargets);
                lockOnProgress = 0;
            }

            if (continuous) targetLocked = true;
        }
    }

    // I hate that this is neccesary
    struct LockOnTarget
    {
        public Target target;
        public float lockOnProgress;

        public void IncrementLockTimer()
        {
            lockOnProgress += Time.deltaTime;
        }
    }
}

// I wish that C# had function pointers because then rather than having if checks I could just generate a list of function pointers in Awake and run through them in update.
// Otherwise I'll have to create a switch with different logic packages or something equally heinous
// Maybe a Tag class with a RunLogic(Tower tower) function? Then a list of Tag variants that get assigned in awake and run in update


