using UnityEngine;
using BuildingList = System.Collections.Generic.List<Building>;

public class Pylon : Building
{
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

            if (currentXP >= XPEnhanceRequirement)
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
                Deactivate();
        }
    }
    public GameObject pylonResidual;

    [Header("Connections")]
    [SerializeField] BuildingList connectedBuildings = new();
    [HideInInspector] public int connectedTowersCount = 0;
    [HideInInspector] public int connectedPylonsCount = 0;

    //[HideInInspector] public Building parent = null;

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
        if (building is Tower)
            connectedTowersCount++;
        else
            connectedPylonsCount++;
    }

    public void RemoveBuilding(Building building)
    {
        connectedBuildings.Remove(building);
        if (building is Tower)
            connectedTowersCount--;
        else
            connectedPylonsCount--;
    }

    public override void Deactivate()
    {
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

        foreach (Building building in connectedBuildings)
        {
            building.Deactivate();
        }
        base.Deactivate();
    }
    public override void Reactivate()
    {
        if (Enhanced)
        {
            enhancedPylon.SetActive(true);
            enhancedBud.SetActive(true);
            deactivatedEnhancedPylon.SetActive(false);
        }
        else
        {
            basePylon.SetActive(true);
            baseBud.SetActive(true);
            deactivatedBasePylon.SetActive(false);
        }


        foreach (Building building in connectedBuildings)
        {
            building.Reactivate();
        }
        base.Reactivate();
    }

    public void ToggleResidual(bool value)
    {
        if (Enhanced)
        {
            pylonResidual.SetActive(value);
            enhancedPylon.SetActive(!value);
            enhancedBud.SetActive(!value);
        }
        else
        {
            pylonResidual.SetActive(value);
            basePylon.SetActive(!value);
            baseBud.SetActive(!value);
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