
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;  
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
    public bool sellFlag;

    [SerializeField] GameObjectList upgradedTowerPrefabs;

    public void Upgrade(int upgradePath)
    {
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

    public override void Sell()
    {
        if (sellFlag)
            return;
        sellFlag = true;
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(cost, sellReturnPercent);
        (parent as Pylon).towerCount--;
        Destroy(gameObject, 0.1f);
    }

}