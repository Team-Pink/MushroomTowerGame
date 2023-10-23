using System.Collections.Generic;
using UnityEngine;
using BuildingList = System.Collections.Generic.List<Building>;

public class Pylon : Building
{
    public MeshRenderer healthDisplay;
    private bool isResidual;

    [Header("Purchasing and Selling")]
    [SerializeField] int costMultiplier = 1;
    [SerializeField, Range(0, 1)] float sellReturnPercent = 0.5f;
    [SerializeField] GameObject basePylon;
    [SerializeField] GameObject baseBud;
    [SerializeField] GameObject deactivatedBasePylon;
    static readonly int baseCost = 10;

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
        bud = baseBud;
        CurrentHealth = pylonHealth;
        AudioManager.PlaySoundEffect(placeAudio.name, 1);
    }

    private void Update()
    {
        GetTowerEXP();// Move this to on wave end in the wave manager when it exists or somewhere else that only triggers a few times a wave.
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


        deactivatedBasePylon.SetActive(true);
        basePylon.SetActive(false);
        baseBud.SetActive(false);
    }
    public override void Reactivate()
    {
        base.Reactivate();
        if (isResidual) return;
        foreach (Building building in connectedBuildings)
        {
            building.Reactivate();
        }

        deactivatedBasePylon.SetActive(false);
        basePylon.SetActive(true);
        baseBud.SetActive(true);
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

    public void SellTower(Tower tower)
    {
        Debug.Log("Sold Tower", tower);
        connectedBuildings.Remove(tower);
        tower.Sell();
    }
}