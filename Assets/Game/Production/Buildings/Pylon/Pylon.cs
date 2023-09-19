using System.Collections.Generic;
using UnityEngine;
using BuildingList = System.Collections.Generic.List<Building>;

public class Pylon : Building
{
    [HideInInspector] bool isResidual;

    [Header("Purchasing and Selling")]
    [SerializeField] int costMultiplier = 1;
    [SerializeField, Range(0, 1)] float sellReturnPercent = 0.5f;
    [SerializeField] GameObject basePylon;
    [SerializeField] GameObject baseBud;
    [SerializeField] GameObject deactivatedBasePylon;
    static readonly int baseCost = 10;

    [Header("Enhancement")]
    [SerializeField] bool enhanced;
    public bool Enhanced
    {
        get;
        private set;
    }
    [SerializeField] GameObject enhancedPylon;
    [SerializeField] GameObject enhancedBud;
    [SerializeField] GameObject deactivatedEnhancedPylon;
    [SerializeField] GameObject pylonPlacementRange;
    [SerializeField] int XPEnhanceRequirement = 2;
    private int currentXP;
    public int CurrentXP
    {
        get => currentXP;
        set
        {
            currentXP = value;

            if (currentXP >= XPEnhanceRequirement * costMultiplier)
            {
                Enhance();
            }
        }
    }
    private int ForceEnhanceCost
    {
        get => 2 * costMultiplier * baseCost;
    }

    [Header("Destruction")]
    [SerializeField] int pylonHealth = 5;
    public int MaxHealth
    {
        get => pylonHealth;
        private set { }
    }
    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;
            if (currentHealth <= 0)
                ToggleResidual(true);
        }
    }

    public GameObject pylonResidual;

    [Header("Connections")]
    [SerializeField] BuildingList connectedBuildings = new();
    public int connectedTowersCount
    {
        get
        {
            int towers = 0;
            HashSet<Building> buildingsToRemove = new HashSet<Building>();

            foreach (var building in connectedBuildings)
            {
                if (building == null)
                {
                    buildingsToRemove.Add(building);
                    continue;
                }
                if (building is Tower)
                    towers++;
            }

            foreach (var building in buildingsToRemove)
                connectedBuildings.Remove(building);

            return towers;
        }
        private set { }
    }
    public int connectedPylonsCount
    {
        get
        {
            int pylons = 0;
            HashSet<Building> buildingsToRemove = new HashSet<Building>();

            foreach (var building in connectedBuildings)
            {
                if (building == null)
                {
                    buildingsToRemove.Add(building);
                    continue;
                }
                if (building is Pylon)
                    pylons++;
            }

            foreach (var building in buildingsToRemove)
                connectedBuildings.Remove(building);

            return pylons;
        }
        private set { }
    }

    public bool IsBuildingInList(Building building)
    {
        return connectedBuildings.Contains(building);
    }

    private void Awake()
    {
        CurrentHealth = pylonHealth;
    }

    private void Update()
    {
        GetTowerEXP();// Move this to on wave end in the wave manager when it exists or somewhere else that only triggers a few times a wave.
    }


    public void Enhance()
    {
        Enhanced = true;
        enhancedPylon.SetActive(true);
        enhancedBud.SetActive(true);

        pylonPlacementRange.SetActive(true);

        basePylon.SetActive(false);
        baseBud.SetActive(false);
    }

    public void AddBuilding(Building building)
    {
        connectedBuildings.Add(building);
    }

    public void RemoveBuilding(Building building)
    {
        connectedBuildings.Remove(building);
    }

    public override void Deactivate()
    {
        if (isResidual) return;

        foreach (Building building in connectedBuildings)
        {
            building.Deactivate();
        }

        if (Enhanced)
        {
            deactivatedEnhancedPylon.SetActive(true);
            enhancedPylon.SetActive(false);
            enhancedBud.SetActive(false);
        }
        else
        {
            deactivatedBasePylon.SetActive(true);
            basePylon.SetActive(false);
            baseBud.SetActive(false);
        }

        base.Deactivate();
    }
    public override void Reactivate()
    {
        if (isResidual) return;

        foreach (Building building in connectedBuildings)
        {
            building.Reactivate();
        }

        if (Enhanced)
        {
            deactivatedEnhancedPylon.SetActive(false);
            enhancedPylon.SetActive(true);
            enhancedBud.SetActive(true);
        }
        else
        {
            deactivatedBasePylon.SetActive(false);
            basePylon.SetActive(true);
            baseBud.SetActive(true);
        }

        base.Reactivate();
    }

    public void ToggleResidual(bool value)
    {
        isResidual = value;

        if (!isResidual)
        {
            foreach (Building connectedBuilding in connectedBuildings)
            {
                connectedBuilding.Reactivate();
            }
        }

        if (Enhanced)
        {
            pylonResidual.SetActive(isResidual);
            deactivatedEnhancedPylon.SetActive(!isResidual);
            enhancedPylon.SetActive(!isResidual);
            enhancedBud.SetActive(!isResidual);
        }
        else
        {
            pylonResidual.SetActive(isResidual);
            deactivatedBasePylon.SetActive(!isResidual);
            basePylon.SetActive(!isResidual);
            baseBud.SetActive(!isResidual);
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
    public int GetForceEnhanceCost()
    {
        return ForceEnhanceCost;
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
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(GetPylonCost(), sellReturnPercent);

        if (connectedBuildings.Count > 0)
        {
            ToggleResidual(true);

            foreach (Building connectedBuilding in connectedBuildings)
            {
                connectedBuilding.Deactivate();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SellAll()
    {
        SellAllConnectedBuildings();

        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        currencyManager.IncreaseCurrencyAmount(GetPylonCost(), sellReturnPercent);

        Sell();
    }

    public void SellAllConnectedBuildings()
    {
        while (connectedBuildings.Count > 0)
        {
            Building building = connectedBuildings[connectedBuildings.Count - 1];
            connectedBuildings.Remove(building);

            if (building == null)
                continue;

            if (building is Pylon)
            {
                (building as Pylon).SellAll();
            }
            else
            {
                building.Sell();
            }
        }
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
            if (total > 0)
                CurrentXP += total;
        }

        return base.GetTowerEXP();
    }
    public void SellTower(Tower tower)
    {
        Debug.Log("Sold Tower", tower);
        connectedBuildings.Remove(tower);
        tower.Sell();
    }
}