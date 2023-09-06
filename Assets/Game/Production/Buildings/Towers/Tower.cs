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

    // Enemy targeter values

    [SerializeField] private float FiringCone = 10;
    // Trap targeter values
    [SerializeField] float TrapRadius = 1;
    [SerializeField] bool FindNumberOfTargets = false;

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
    public bool accelerated = false; // determines if a tower is currently accelerated
    readonly float accelTimeMax = 5; // the time a tower will go without killing before accelerate resets
    public float accelTimer = 0; // timer to keep track of the above.
    public readonly float accelSpeedMod = 0.5f; // on kill multiply the attack delay by this basically increase by 50%
    public readonly float decreaseAccel = 1.25f; //acceleration decreases by 25% if the tower fails a kill.
    private float baseDelay; // the original starting delay of the tower
    public bool GetAccelerate() => accelerate; // determines if a tower can accelerate

    private void Awake()
    {
        animator = GetComponent<Animator>();

        transform = gameObject.transform;
        targeterComponent.transform = transform;
        attackerComponent.transform = transform;

        targeterComponent.enemyLayer = LayerMask.GetMask("Enemy");

        if (targeterComponent is TrackTargeter)
        {
            (targeterComponent as TrackTargeter).layerMask = LayerMask.GetMask("Ground", "NotPlaceable"); // for the ink tower to differentiate path
            (targeterComponent as TrackTargeter).trapRadius = TrapRadius;
            (targeterComponent as TrackTargeter).findNumberOfTargets = FindNumberOfTargets;
        }
        else
        {
            (targeterComponent as EnemyTargeter).firingCone = FiringCone;
        }

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
}

// if you aren't going to implement it functionally don't add it to alpha.
//#if UNITY_EDITOR
//namespace Editor
//{
//    using UnityEditor;
//    [CustomEditor(typeof(Tower))]
//    public class TowerEditor : Editor
//    {
//       // public override void OnInspectorGUI()
//        {
//           // GUILayout.Button("Open Editor", GUILayout.MaxWidth(50));
//        }
//    }
//}
//#endif
