using System.Collections.Generic;
using UnityEngine;
using BuildingList = System.Collections.Generic.List<Building>;

public class Pylon : Building
{
    public MeshRenderer healthDisplay;
    public bool isResidual
    {
        get;
        private set;
    }

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
    [SerializeField] float pylonHealth = 5;
    public float MaxHealth
    {
        get => pylonHealth;
        private set { }
    }
    private float currentHealth;
    public float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;
            if (currentHealth <= float.Epsilon)
            {
                AudioManager.PlaySoundEffect(deathAudio.name, 1);
                if (CanTurnIntoResidual())
                    ToggleResidual(true);
            }
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

    [Header("Sounds")]
    [SerializeField] AudioClip placeAudio;
    [SerializeField] AudioClip deathAudio;

    public bool IsBuildingInList(Building building)
    {
        return connectedBuildings.Contains(building);
    }

    private void Start()
    {
        CurrentHealth = pylonHealth;
        AudioManager.PlaySoundEffect(placeAudio.name, 1);
    }

    private void Update()
    {
        GetTowerEXP();// Move this to on wave end in the wave manager when it exists or somewhere else that only triggers a few times a wave.

        if (isResidual && connectedPylonsCount == 0 && connectedTowersCount == 0)
            Destroy(gameObject);
    }


    public void Enhance()
    {
        Enhanced = true;
        if (!isResidual)
        {
            enhancedPylon.SetActive(true);
            enhancedBud.SetActive(true);
        }

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
        base.Deactivate();
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
    }
    public override void Reactivate()
    {
        base.Reactivate();
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
    }

    public void ToggleResidual(bool value)
    {
        isResidual = value;

        if (isResidual)
        {
            foreach (Building connectedBuilding in connectedBuildings)
            {
                connectedBuilding.Deactivate();
            }
        }
        else if (Active)
        {
            foreach (Building connectedBuilding in connectedBuildings)
            {
                connectedBuilding.Reactivate();
            }
        }

        if (Enhanced)
        {
            if (Active)
            {
                enhancedPylon.SetActive(!isResidual);
                enhancedBud.SetActive(!isResidual);
            }
            else
            {
                deactivatedEnhancedPylon.SetActive(!isResidual);
            }
            pylonResidual.SetActive(isResidual);
        }
        else
        {
            if (Active)
            {
                basePylon.SetActive(!isResidual);
                baseBud.SetActive(!isResidual);
            }
            else
            {
                deactivatedBasePylon.SetActive(!isResidual);
            }
            pylonResidual.SetActive(isResidual);
        }
    }
    public bool CanTurnIntoResidual()
    {
        if (connectedBuildings.Count > 0)
            return true;
        else
            Destroy(gameObject);
        return false;
    }

    #region PYLON COST
    public int GetPylonCost() => baseCost * (costMultiplier);
    public int GetPylonCost(int instance) => baseCost * (instance);
    public int GetForceEnhanceCost() => ForceEnhanceCost;
    public int GetMultiplier() => costMultiplier;
    public void SetMultiplier(int number) => costMultiplier = number;
    public static int GetPylonBaseCurrency() => baseCost;
    public int GetPylonSellAmount()
    {
        if (isResidual == false)
            return (int)((baseCost * costMultiplier) * sellReturnPercent);
        else
            return 0;
    }
    public int GetPylonSellAllAmount()
    {
        int returnCost = 0;
        List<Pylon> openList = new List<Pylon>();
        bool exitLoop = false;

        openList.Add(this);

        while(!exitLoop)
        {
            if (openList.Count == 0)
            {
                exitLoop = true;
                continue;
            }

            Pylon pylon = openList[0];

            if (pylon.connectedBuildings.Count <= 0)
            {
                if (pylon.isResidual == false)
                    returnCost += pylon.GetPylonSellAmount();
                openList.Remove(pylon);
                continue;
            }

            foreach (Building building in pylon.connectedBuildings)
            {
                if (building is Pylon)
                    openList.Add(building as Pylon);
                else
                    returnCost += (building as Tower).SellPrice();
            }

            if (pylon.isResidual == false)
                returnCost += pylon.GetPylonSellAmount();

            openList.Remove(pylon);
        }
        
        return returnCost;
    }
    #endregion


    public override void Sell()
    {
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();

        if (!isResidual)
            currencyManager.IncreaseCurrencyAmount(GetPylonCost(), sellReturnPercent);

        if (connectedBuildings.Count > 0)
            ToggleResidual(true);
        else
            Destroy(gameObject);
    }
    public void SellAll()
    {
        SellAllConnectedBuildings();
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