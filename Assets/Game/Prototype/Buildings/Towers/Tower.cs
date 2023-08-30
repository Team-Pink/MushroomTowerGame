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

[Serializable] public enum TargeterType
{
    Close,
    Cluster,
    Fast,
    Strong,
    Track
} // For Editor Use Only
[Serializable] public enum AttackerType
{
    Area,
    Single,
    Trap
} // For Editor Use Only

[Serializable] public struct Details
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

    // Tower Components
    public TurretController TowerController;

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

    private void Awake()
    {
        transform = gameObject.transform;
        //TowerController = transform.GetChild(2).gameObject.GetComponent<TurretController>();

        targeterComponent.transform = transform;
        targeterComponent.enemyLayer = LayerMask.GetMask("Enemy");

        if (targeterComponent is TrackTargeter)
            (targeterComponent as TrackTargeter).layerMask = LayerMask.GetMask("Ground", "NotPlaceable"); // for the ink tower to differentiate path
    }

    private void Update()
    {
        if (Active)
        {
            targets = targeterComponent.AcquireTargets();
            attackerComponent.Attack(targets);
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
}