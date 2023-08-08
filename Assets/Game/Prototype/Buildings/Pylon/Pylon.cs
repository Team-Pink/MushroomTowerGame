using System.Collections.Generic;
using UnityEngine;

using BuildingList = System.Collections.Generic.List<Building>;
using ContextMenu = UnityEngine.ContextMenu;

using Debug = UnityEngine.Debug;

public class Pylon : Building
{
    public int costMultiplier = 1;
    public static int baseCost = 10;
    [SerializeField, Range(0, 1)] float sellReturnPercent = 0.5f;
    public int forceEnhanceCost = 20;

    private BuildingList connectedBuildings = new();
    private List<Tower> connectedTowerList = new();
    private int buildingCount;
    private int EXPforLVLup = 2;
    [SerializeField] private bool enhanced;
    public bool Enhanced { get; private set; }

    [SerializeField] private int pylonEXP;
    public int EXP
    {
        get
        {
            return pylonEXP;
        }
        set
        {
            pylonEXP = value;
            CheckIfEnhanced();
        }
    }

    private void CheckIfEnhanced()
    {
        if (pylonEXP >= EXPforLVLup)
        {
            Enhance();
        }
    }
    public bool IsBuildingInList(Building building)
    {
        return connectedBuildings.Contains(building);
    }

    private void Update()
    {
        for (int buildingIndex = 0; buildingIndex < connectedBuildings.Count; buildingIndex++)
        {
            Building building = connectedBuildings[buildingIndex];
            if (building is Tower)
            {
                if ((building as Tower).sellFlag)
                    SellTower(building as Tower);
            }
        }
        GetTowerEXP();// Move this to on wave end in the wave manager when it exists or somewhere else that only triggers a few times a wave.
    }


    public void Enhance() => Enhanced = true;
    public void AddBuilding(Building building) => connectedBuildings.Add(building);

    [ContextMenu("Deactivate")] public override void Deactivate()
    {
        DeactivateConnectedBuildings();

        base.Deactivate();
    }
    public void DeactivateConnectedBuildings()
    {
        foreach (Building building in connectedBuildings)
        {
            building.Deactivate();
        }
    }
    public override void Reactivate()
    {
        base.Reactivate();

        foreach (Building building in connectedBuildings)
        {
            building.Reactivate();
        }
    }

#region PYLON COST
    public int GetPylonCost()
    {
        return baseCost * (costMultiplier);
    }
    public int GetPylonCost(int instance)
    {
        return baseCost * (instance);
    }
    public int GetMultiplier()
    {
        return costMultiplier;
    }
    public void SetMultiplier(int number)
    {
        costMultiplier = number;
    }
    public static int GetPylonBaseCurrency()
    {
        return baseCost;
    }
#endregion

    /// <summary>
    /// generate a list of Tower from the building list 
    /// </summary>
    private void GenerateTowerList()
    {
        buildingCount = connectedBuildings.Count;// set buildingcount.

        connectedTowerList.Clear(); // clobber the existing list
        // generate a list of towers
        foreach (Building building in connectedBuildings)
        {
            if (building.gameObject.GetComponent<Tower>())
            {
                connectedTowerList.Add(building.gameObject.GetComponent<Tower>());
            }
        }
    }
    public void GetTowerEXP()
    {
        if (buildingCount < connectedBuildings.Count)
            GenerateTowerList();
        foreach (Tower tower in connectedTowerList)
        {
            if (tower.TowerController.storedExperience > 0)
            { 
            EXP += tower.TowerController.storedExperience;
            tower.TowerController.storedExperience = 0;
            }
        }
    } 
    public override void Sell()
    {
        DeactivateConnectedBuildings();

        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(GetPylonCost(), sellReturnPercent);

        base.Sell();
    }
    public void SellTower(Tower tower)
    {
        Debug.Log("Sold Tower", tower);
        connectedBuildings.Remove(tower);
        tower.Sell();
    }
    public void SellAll()
    {
        BuildingList sellList = new();

        for (int buildingIndex = 0; buildingIndex < connectedBuildings.Count; buildingIndex++)
        {
            Building building = connectedBuildings[buildingIndex];
            if (building is Pylon)
                (building as Pylon).SellAll();
            else
            {
                sellList.Add(building);
            }
        }

        for (int buildingIndex = 0; buildingIndex < sellList.Count; buildingIndex++)
        {
            SellTower(sellList[buildingIndex] as Tower);
        }

        Sell();
    }
}