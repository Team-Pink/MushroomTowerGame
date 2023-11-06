using System.Collections.Generic;
using UnityEngine;

public class Node : Building
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
    [SerializeField] List<Building> connectedBuildings = new();
    public int connectedShroomsCount
    {
        get
        {
            int shroomCount = 0;
            HashSet<Building> buildingsToRemove = new HashSet<Building>();

            foreach (var building in connectedBuildings)
            {
                if (building == null)
                {
                    buildingsToRemove.Add(building);
                    continue;
                }
                if (building is Shroom)
                    shroomCount++;
            }

            foreach (var building in buildingsToRemove)
                connectedBuildings.Remove(building);

            return shroomCount;
        }
        private set { }
    }
    public int connectedNodesCount
    {
        get
        {
            int nodes = 0;
            HashSet<Building> buildingsToRemove = new HashSet<Building>();

            foreach (var building in connectedBuildings)
            {
                if (building == null)
                {
                    buildingsToRemove.Add(building);
                    continue;
                }
                if (building is Node)
                    nodes++;
            }

            foreach (var building in buildingsToRemove)
                connectedBuildings.Remove(building);

            return nodes;
        }
        private set { }
    }

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
        if (isResidual && connectedNodesCount == 0 && connectedShroomsCount == 0)
            Destroy(gameObject);

        bool showBud = !(budDetached) && !isResidual;
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

    #region NODE COST
    public int GetNodeCost() => baseCost * (costMultiplier);
    public int GetNodeCost(int instance) => baseCost * (instance);
    public int GetMultiplier() => costMultiplier;
    public void SetMultiplier(int number) => costMultiplier = number;
    public static int GetNodeBaseCurrency() => baseCost;
    public int GetNodeSellAmount()
    {
        if (isResidual == false)
            return (int)((baseCost * costMultiplier) * sellReturnPercent);
        else
            return 0;
    }
    public int GetNodeSellAllAmount()
    {
        int returnCost = 0;
        List<Node> openList = new List<Node>();
        bool exitLoop = false;

        openList.Add(this);

        while(!exitLoop)
        {
            if (openList.Count == 0)
            {
                exitLoop = true;
                continue;
            }

            Node node = openList[0];

            if (node.connectedBuildings.Count <= 0)
            {
                if (node.isResidual == false)
                    returnCost += node.GetNodeSellAmount();
                openList.Remove(node);
                continue;
            }

            foreach (Building building in node.connectedBuildings)
            {
                if (building is Node)
                    openList.Add(building as Node);
                else
                    returnCost += (building as Shroom).SellPrice();
            }

            if (node.isResidual == false)
                returnCost += node.GetNodeSellAmount();

            openList.Remove(node);
        }
        
        return returnCost;
    }
    #endregion


    public override void Sell()
    {
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();

        if (!isResidual)
            currencyManager.IncreaseCurrencyAmount(GetNodeCost(), sellReturnPercent);

        if (connectedBuildings.Count > 0)
            ToggleResidual(true);
        else
            Destroy(gameObject);
    }

    public void SellShroom(Shroom shroom)
    {
        Debug.Log("Sold Shroom", shroom);
        connectedBuildings.Remove(shroom);
        shroom.Sell();
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
            if (building is not Shroom) building.ShowDeactivateLines();
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
            if (building is not Shroom) building.ShowSellLines();
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
            if (building is not Shroom) building.ResetLines();
        }
    }
}