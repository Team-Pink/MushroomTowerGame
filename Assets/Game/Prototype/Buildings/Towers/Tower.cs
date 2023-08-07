<<<<<<< HEAD
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Tower : Building
{
    public TurretController TowerController; // this  seems like a good place to leave a reference to the individual tower functionality.
    // should strip the tower controller for parts for this later.




#if UNITY_EDITOR
    private void Update()
    {
       EditorApplication.Beep(); // Why Connor?
    }  
#endif
=======
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
>>>>>>> bd1fdbe35e0e26cdee5d69b7a12991881e13b72c
}