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

    // Temp Variables
    [SerializeField] GameObject bulletPrefab;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        transform = gameObject.transform;
        targeterComponent.transform = transform;
        attackerComponent.transform = transform;
        
        targeterComponent.enemyLayer = LayerMask.GetMask("Enemy");

        attackerComponent.bulletPrefab = bulletPrefab;

        radiusDisplay.transform.localScale = new Vector3(2 * targeterComponent.range, 2 * targeterComponent.range);
    }

    private void Update()
    {
        if (Active)
        {
            targets = targeterComponent.AcquireTargets();
            if (targets != null)
            {
                attackerComponent.Attack(targets);
                
                // rotate tower to targetted enemy
                foreach (Target targetEnemy in targets)
                {
                    // take enemy experience
                    if (targetEnemy.enemy != null)
                    {
                        storedExperience += targetEnemy.enemy.expValue;
                        targetEnemy.enemy.expValue = 0;
                    }

                    // remove it from targets and retarget
                }
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
