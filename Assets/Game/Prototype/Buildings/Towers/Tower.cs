using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

public class Tower : Building
{

    // Components
    protected new Transform transform;
    protected Animator animator;
    [SerializeField] private Attacker attackerComponent;

    // Targeter
    [SerializeField] private Targeter targeterComponent;
    [SerializeField] private float TurnRate;
    [SerializeField] private float FiringCone;

    // References
    private HashSet<Target> targets = new HashSet<Target>();


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

    private void Awake()
    {
        transform = gameObject.transform; // must be at top.

        targeterComponent.transform = transform;
        targeterComponent.enemyLayer = LayerMask.GetMask("Enemy");

        if (targeterComponent is TrackTargeter)
        {
            (targeterComponent as TrackTargeter).layerMask = LayerMask.GetMask("Ground", "NotPlaceable"); // for the ink tower to differentiate path
        }
        else
        {
            (targeterComponent as EnemyTargeter).turnRate = TurnRate;
            (targeterComponent as EnemyTargeter).firingCone = FiringCone;
        }
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
                    if (targetEnemy.enemy.isDead)
                    {
                        // take enemy experience
                        storedExperience += targetEnemy.enemy.expValue;
                        targetEnemy.enemy.expValue = 0;

                        // remove it from targets and retarget

                    }
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

#if UNITY_EDITOR
namespace Editor
{
    using UnityEditor;
    [CustomEditor(typeof(Tower))]
    public class TowerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Button("Open Editor", GUILayout.MaxWidth(50));
        }
    }
}
#endif