using System.Collections.Generic;
using System.ComponentModel.Design;
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

    [HideInInspector] public bool budDetached = false;

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
                if (CanTurnIntoResidual())
                    ToggleResidual(true);
            }
        }
    }

    public GameObject pylonResidual;

    [Header("Connections")]
    [SerializeField] BuildingList connectedBuildings = new();
    public bool AtMaxBuildings { get => AtMaxTowers && AtMaxPylons; }
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
    private bool atMaxTowers;
    public bool AtMaxTowers { get => atMaxTowers; }
    private bool atMaxPylons;
    public bool AtMaxPylons { get => atMaxPylons; }

    public GameObject displayLinePrefab;
    public Material displayLineDefault;
    public Material displayLineSell;
    public Material displayLineDeactivate;
    private List<GameObject> displayLines = new();
    private Vector3 lineRendererOffset = new(0, 1.0f, 0);


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
        if (isResidual && connectedPylonsCount == 0 && connectedTowersCount == 0)
            Destroy(gameObject);

        if (atMaxTowers != (connectedTowersCount == InteractionManager.pylonMaxTowers))
        {
            atMaxTowers = connectedTowersCount == InteractionManager.pylonMaxTowers;
            radiusDisplay.transform.GetChild(2).gameObject.SetActive(!atMaxTowers);
        }
        if (atMaxPylons != (connectedPylonsCount == InteractionManager.pylonMaxPylons))
        {
            atMaxPylons = connectedPylonsCount == InteractionManager.pylonMaxPylons;
            radiusDisplay.transform.GetChild(0).gameObject.SetActive(!atMaxPylons);
            radiusDisplay.transform.GetChild(1).gameObject.SetActive(!atMaxPylons);
        }

        bool showBud = !(AtMaxBuildings || budDetached) && !isResidual;
        bud.SetActive(showBud);
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

    public void SellTower(Tower tower)
    {
        Debug.Log("Sold Tower", tower);
        connectedBuildings.Remove(tower);
        tower.Sell();
    }


    public override void ShowDefaultLines()
    {
        ResetLines();

        for (int i = 0; i < connectedBuildings.Count; i++)
        {
            Building building = connectedBuildings[i];

            GameObject line = Instantiate(displayLinePrefab, transform);
            displayLines.Add(line);

            LineRenderer renderer = line.GetComponent<LineRenderer>();
            renderer.material = displayLineDefault;
            renderer.SetPosition(0, building.transform.position - transform.position + lineRendererOffset);
            renderer.SetPosition(1, lineRendererOffset);
        }
    }
    public override void ShowDeactivateLines()
    {
        ResetLines();

        //set lines here
        for (int i = 0; i < connectedBuildings.Count; i++)
        {
            Building building = connectedBuildings[i];

            GameObject line = Instantiate(displayLinePrefab, transform);
            displayLines.Add(line);

            LineRenderer renderer = line.GetComponent<LineRenderer>();
            renderer.material = displayLineDeactivate;
            renderer.SetPosition(0, building.transform.position - transform.position + lineRendererOffset);
            renderer.SetPosition(1, lineRendererOffset);

            //Recurse through children
            if (building is not Tower) building.ShowDeactivateLines();
        }
    }
    public override void ShowSellLines()
    {
        ResetLines();

        //set lines here
        for (int i = 0; i < connectedBuildings.Count; i++)
        {
            Building building = connectedBuildings[i];

            GameObject line = Instantiate(displayLinePrefab, transform);
            displayLines.Add(line);

            LineRenderer renderer = line.GetComponent<LineRenderer>();
            renderer.material = displayLineSell;
            renderer.SetPosition(0, building.transform.position - transform.position + lineRendererOffset);
            renderer.SetPosition(1, lineRendererOffset);

            //Recurse through children
            if (building is not Tower) building.ShowSellLines();
        }
    }
    public override void ResetLines()
    {
        int lineAmount = displayLines.Count;
        for (int i = 0; i < lineAmount; i++)
        {
            Destroy(displayLines[i]);
        }
        displayLines.Clear();

        //Recurse through children
        for (int i = 0; i < connectedBuildings.Count; i++)
        {
            Building building = connectedBuildings[i];
            if (building is not Tower) building.ResetLines();
        }
    }
}