using System.Collections.Generic;
using UnityEngine;

using BuildingList = System.Collections.Generic.List<Building>;
using ContextMenu = UnityEngine.ContextMenu;

using Debug = UnityEngine.Debug;

public class Pylon : Building
{
    private int counter = 0;
    [HideInInspector]
    public int towerCount = 0; //used to limit the number of placeable towers from a pylon.
    [HideInInspector]
    public int pylonCount = 0; //used to limit the number of placeable pylons from a pylon.

    [HideInInspector]
    public Building parent = null; // as far as I can tell this is only ever set in the interaction manager but never used.

    [SerializeField] private int costMultiplier = 1;
    private static int baseCost = 10;
    [SerializeField, Range(0, 1)] float sellReturnPercent = 0.5f;
    [SerializeField] private int forceEnhanceCost = 20;

    [SerializeField] private BuildingList connectedBuildings = new();
    private int EXPforLVLup = 2;
    [SerializeField] private bool enhanced; public bool Enhanced { get; private set; }

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
        GetTowerEXP();// Move this to on wave end in the wave manager when it exists or somewhere else that only triggers a few times a wave.

    }


    public void Enhance()
    {
        Enhanced = true;
        transform.GetChild(2).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(false);
    }

    public void AddBuilding(Building building) => connectedBuildings.Add(building);
    public void RemoveBuilding(Building building) => connectedBuildings.Remove(building);

    [ContextMenu("Deactivate")]
    public override void Deactivate()
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
        if (Enhanced) transform.GetChild(2).gameObject.SetActive(true);
        else transform.GetChild(1).gameObject.SetActive(true);
        transform.GetChild(4).gameObject.SetActive(true);
        

        foreach (Building building in connectedBuildings)
        {
            building.Reactivate();
        }
        base.Reactivate();
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
    public int GetForceEnhanceCost()
    {
        return forceEnhanceCost;
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


    public override void Sell()
    {
        Deactivate();

        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(GetPylonCost(), sellReturnPercent);

        if (connectedBuildings.Count > 0)
            base.SellAll();
        else
            base.Sell();
    }

    public override int GetTowerEXP()
    {
        if (!enhanced && connectedBuildings.Count > 0)
        {
            int total = 0;
            foreach (Building building in connectedBuildings)
            {
                total += building.GetTowerEXP();
            }
            if (total > 0) EXP += total;
            counter++;
        }

        return base.GetTowerEXP();
    }
    public void SellTower(Tower tower)
    {
        Debug.Log("Sold Tower", tower);
        connectedBuildings.Remove(tower);
        tower.Sell();
    }
    public override void SellAll()
    {
        SellAllConnectedBuildings();

        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(GetPylonCost(), sellReturnPercent);

        base.SellAll();
    }

    public void SellAllConnectedBuildings()
    {
        foreach (Building building in connectedBuildings)
        {
            building.SellAll();
        }
    }
}