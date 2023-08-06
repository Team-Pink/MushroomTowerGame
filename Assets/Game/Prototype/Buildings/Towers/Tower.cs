using Debug = UnityEngine.Debug;
using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using UnityEngine;
public class Tower : Building
{
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
        sellFlag = true;
        Destroy(gameObject, 0.1f);
    }
}