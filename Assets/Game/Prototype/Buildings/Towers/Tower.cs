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

public class Tower : Building
{

    // Components
    protected new Transform transform;
    protected Animator animator;
    [SerializeField] private Attacker attackerComponent = new SingleAttacker();

    // Targeter
    [SerializeField] private Targeter targeterComponent = new StrongTargeter();
    // Enemy targeter values
    [SerializeField] private float TurnRate = 5;
    [SerializeField] private float FiringCone = 10;
    // Trap targeter values
    [SerializeField] float TrapRadius = 1;
    [SerializeField] bool FindNumberOfTargets = false;

    public Attacker AttackerComponent { get => attackerComponent; set => attackerComponent = value; }

    public Targeter TargeterComponent { get => targeterComponent; set => targeterComponent = value; }

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

    // Temp Variables
    [SerializeField] GameObject bulletPrefab;

    private void Awake()
    {
        
        transform = gameObject.transform; // must be at top.
        //if Tower does not have a targeter type assigned it will case an null reference error when instantiating it.       
        targeterComponent.transform = transform;

        
        targeterComponent.enemyLayer = LayerMask.GetMask("Enemy");

        if (targeterComponent is TrackTargeter)
        {
            (targeterComponent as TrackTargeter).layerMask = LayerMask.GetMask("Ground", "NotPlaceable"); // for the ink tower to differentiate path
            (targeterComponent as TrackTargeter).trapRadius = TrapRadius;
            (targeterComponent as TrackTargeter).findNumberOfTargets = FindNumberOfTargets;
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
                    else AnimateAttack(targetEnemy);
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

    public void AnimateAttack(Target target)
    {
        GameObject bulletRef;

      
        bulletRef = Instantiate(bulletPrefab, transform.position + Vector3.up * 2, Quaternion.identity);

        bulletRef.GetComponent<Bullet>().timeToTarget = attackerComponent.attackDelay;
        bulletRef.GetComponent<Bullet>().target = target;

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
