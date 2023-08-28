using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public struct Target
{
    public Vector3 position;
    public Enemy enemy;
}

public abstract class Targeter // Lochlan you remove this and replace with yours
{
    public abstract HashSet<Target> AcquireTargets();
}

public class Tower : Building
{
    // Components
    protected new Transform transform;
    protected Animator animator;
    [SerializeField] private Attacker attackerComponent;
    private Targeter targeterComponent;

    // References
    private HashSet<Target> targets = new HashSet<Target>();

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

    //Misc/Testing (delete when targetting is set up)
    [SerializeField]
    Transform testObject;
    public enum AttackType
    {
        SingleAttacker,
        AreaAttacker,
        TrapAttacker
    }
    [SerializeField]
    AttackType attackType;
    //End Delete

    private void Awake()
    {
        transform = gameObject.transform;

        //remove here
        testObject = GameObject.Find("TestObject").transform;
        switch (attackType)
        {
            case AttackType.SingleAttacker:
                attackerComponent = new SingleAttacker();
                break;
            case AttackType.AreaAttacker:
                attackerComponent = new AreaAttacker();
                break;
            case AttackType.TrapAttacker:
                attackerComponent = new TrapAttacker();
                break;
        }
        //end remove
    }

    private void Update()
    {
        if (Active)
        {
            //targets = targeterComponent.AcquireTargets();

            //delete all when targetting system's all good
            Target newTarget = new Target();
            newTarget.position = testObject.transform.position;
            targets.Add(newTarget);
            attackerComponent.Attack(targets);
            targets.Clear();
            //end delete
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

#if UNITY_EDITOR
namespace Editor
{
    using UnityEditor;
    [CustomEditor(typeof(Tower))]
    public class TowerEditor : Editor
    {
        //public override void OnInspectorGUI()
        //{
        //    GUILayout.Button("Open Editor", GUILayout.MaxWidth(50));
        //}
    }
}
#endif