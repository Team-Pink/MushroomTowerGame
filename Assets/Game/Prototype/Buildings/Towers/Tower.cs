
using UnityEngine;
using Debug = UnityEngine.Debug;
using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;

public class Tower : Building
{
    public int cost = 10;
    [SerializeField, Range(0,1)] float sellReturnPercent = 0.5f;

    [HideInInspector]
    public Building parent = null;

    public TurretController TowerController;

    [SerializeField] bool upgraded;
    public bool Upgraded { get; private set; }

    [SerializeField] GameObjectList upgradedTowerPrefabs;

    private void Awake()
    {
        TowerController = transform.GetChild(2).gameObject.GetComponent<TurretController>();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        TowerController.gameObject.SetActive(false);
    }

    public override void Reactivate()
    {
        base.Reactivate();

        TowerController?.gameObject.SetActive(true);
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

(parent as Pylon).towerCount--;

        base.SellAll();
    }

    public override void SellAll()
    {
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(cost, sellReturnPercent);
        
        base.SellAll();
    }

    public override int GetTowerEXP()
    {
        if (!TowerController) return 0;
        int exp = TowerController.storedExperience;
        TowerController.storedExperience = 0;
        return exp;
    }
}