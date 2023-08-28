using System.Collections.Generic;
using UnityEngine;
using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;

public class Tower : Building
{

    public int cost = 10;
    [SerializeField, Range(0,1)] float sellReturnPercent = 0.5f;

    [HideInInspector]
    public Building parent = null;

    // Tower Components
    public TurretController TowerController;
    private Targeter targeter= new ClusterTargeter();
    // insert attacker here
    

    [SerializeField] bool upgraded;
    public bool Upgraded { get; private set; }

    [SerializeField] GameObjectList upgradedTowerPrefabs;

    private void Awake()
    {
        TowerController = transform.GetChild(2).gameObject.GetComponent<TurretController>();
        targeter.transform = transform;
        targeter.enemyLayer = LayerMask.GetMask("Enemy");
        //(targeter as TrackTargeter).layerMask = LayerMask.GetMask("Ground", "NotPlaceable");
        
    }

    private void Update()
    {
        targeter.GetTargetsInRange();
        targeter.AcquireTargets(1);
        // update attacker.
    }

    public void Upgrade(int upgradePath)
    {
       // if (!Active) TowerController.enabled = false;
       // else TowerController.enabled = true;
            
        if (upgradePath >= 0 && upgradePath < upgradedTowerPrefabs.Count)
        {
            Transform transform = gameObject.transform;
            Instantiate(upgradedTowerPrefabs[upgradePath], transform.position, transform.rotation, transform.parent);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Upgrade only accepts an int value between 0 and its amount of possible upgrades (" + upgradedTowerPrefabs.Count + ").", this);
        }
    }

    public override void Deactivate()
    {
        TowerController.enabled = false;
        base.Deactivate();
    }

    public override void Reactivate()
    {
        TowerController.enabled = true;
        base.Reactivate();
    }

    public override void Sell()
    {
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(cost, sellReturnPercent);

        (parent as Pylon).connectedTowersCount--;

        Destroy(gameObject);

        base.Sell();
    }

    public override int GetTowerEXP()
    {
        if (!TowerController) return 0;
        int exp = TowerController.storedExperience;
        TowerController.storedExperience = 0;
        return exp;
    }

    
}