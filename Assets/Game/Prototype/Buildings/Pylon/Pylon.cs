using System.Collections.Generic;
using UnityEngine;

using BuildingList = System.Collections.Generic.List<Building>;
using ContextMenu = UnityEngine.ContextMenu;

using Debug = UnityEngine.Debug;

public class Pylon : Building
{
    [HideInInspector] 
    public int towerCount = 0;
    [HideInInspector] 
    public int pylonCount = 0;

    [HideInInspector]
    public Building parent = null;

    public int costMultiplier = 1;
    public static int baseCost = 10;
    [SerializeField, Range(0, 1)] float sellReturnPercent = 0.5f;
    public int forceEnhanceCost = 20;

    [SerializeField] private BuildingList connectedBuildings = new();
    [SerializeField] private List<Tower> connectedTowerList = new();
    private int buildingCount;
    private int EXPforLVLup = 2;
    [SerializeField] private bool enhanced;public bool Enhanced { get; private set; }

    [SerializeField] private int pylonHealth = 5;
    public int PylonHealth { get { return pylonHealth; } set { pylonHealth = value; /**/ if (pylonHealth <= 0) Deactivate(); } }

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


    public void Enhance()
    {
Enhanced = true;
        transform.GetChild(2).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(false);
    }

    public void AddBuilding(Building building) => connectedBuildings.Add(building);

    [ContextMenu("Deactivate")] public override void Deactivate()
    {
        DeactivateConnectedBuildings();

        transform.GetChild(3).gameObject.SetActive(true);
        if (Enhanced) transform.GetChild(2).gameObject.SetActive(false);
        else transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(4).gameObject.SetActive(false);
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
        transform.GetChild(3).gameObject.SetActive(false);
        if(Enhanced) transform.GetChild(2).gameObject.SetActive(true);
        else transform.GetChild(1).gameObject.SetActive(true);
        transform.GetChild(4).gameObject.SetActive(true);
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

        if (parent is not Hub)
        {
            (parent as Pylon).pylonCount--;
            (parent as Pylon).connectedBuildings.Remove(this);
        }
        else
        {
            (parent as Hub).pylonCount--;
        } 

        

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
        for (int buildingIndex = connectedBuildings.Count - 1; buildingIndex >= 0; buildingIndex--)
        {
            Building building = connectedBuildings[buildingIndex];
            if(building != null)
            {
                if (building is Pylon)
                    (building as Pylon).SellAll();
                else
                {
                    SellTower(building as Tower);
                }
            }
        }
        Sell();
    }
}