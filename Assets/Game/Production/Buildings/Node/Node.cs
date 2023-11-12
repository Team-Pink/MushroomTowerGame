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
    [SerializeField] GameObject nodeMesh;
    [SerializeField] GameObject deactivatedMesh;
    static readonly int baseCost = 10;

    [Header("Destruction")]
    [SerializeField] float nodeHealth = 5;
    public float MaxHealth
    {
        get => nodeHealth;
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

    public GameObject nodeResidual;
    [SerializeField] GameObject regrowCanvas;

    [Header("Connections")]
    [SerializeField] List<Building> connectedBuildings = new();
    public int connectedShroomsCount
    {
        get
        {
            int shroomCount = 0;
            HashSet<Building> buildingsToRemove = new();

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
            HashSet<Building> buildingsToRemove = new();

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
    private readonly List<GameObject> displayLines = new();
    private Vector3 lineRendererOffset = new(0, 1.0f, 0);


    [Header("Sounds")]
    [SerializeField] AudioClip placeAudio;
    [SerializeField] AudioClip deathAudio;

    private void Start()
    {
        CurrentHealth = nodeHealth;
        AudioManager.PlaySoundEffect(placeAudio.name, 1);
    }

    private void Update()
    {
        if (isResidual && connectedNodesCount == 0 && connectedShroomsCount == 0)
            Destroy(gameObject);

        bool showBud = !budDetached && !isResidual && Active;
        bud.SetActive(showBud);
    }

    public void AddBuilding(Building building) => connectedBuildings.Add(building);
    public void RemoveBuilding(Building building) => connectedBuildings.Remove(building);

    public override void Deactivate()
    {
        base.Deactivate();
        if (isResidual) return;
        foreach (Building building in connectedBuildings)
        {
            building.Deactivate();
        }

        deactivatedMesh.SetActive(true);
        nodeMesh.SetActive(false);
    }
    public override void Reactivate()
    {
        base.Reactivate();
        if (isResidual) return;
        foreach (Building building in connectedBuildings)
        {
            building.Reactivate();
        }

        deactivatedMesh.SetActive(false);
        nodeMesh.SetActive(true);
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
            nodeMesh.SetActive(!isResidual);
        }
        else
        {
            deactivatedMesh.SetActive(!isResidual);
        }
        nodeResidual.SetActive(isResidual);

        regrowCanvas.SetActive(isResidual);
    }
    public bool CanTurnIntoResidual()
    {
        RemoveNullBuildings();

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

    public void SetCursorCostToResidualCost()
    {
        CursorManager cursorManager = GameObject.Find("GameManager").GetComponent<CursorManager>();
        cursorManager.DisplayCost(GetNodeCost());

        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        if (currencyManager.CanDecreaseCurrencyAmount(GetNodeCost()))
            cursorManager.ChangeCursor("CanBuy");
        else
            cursorManager.ChangeCursor("CannotBuy");
    }
    public void SetCursorCostToNull()
    {
        CursorManager cm = GameObject.Find("GameManager").GetComponent<CursorManager>();
        cm.DisplayCost();
        cm.ChangeCursor("Default");
    }
    public void CheckIfCanToggleResidual()
    {
        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();
        if (currencyManager.CanDecreaseCurrencyAmount(GetNodeCost()))
        {
            currencyManager.DecreaseCurrencyAmount(GetNodeCost());
            ToggleResidual(false);
        }
    }
    #endregion


    public override void Sell()
    {
        RemoveNullBuildings();

        CurrencyManager currencyManager = GameObject.Find("GameManager").GetComponent<CurrencyManager>();

        if (!isResidual)
            currencyManager.IncreaseCurrencyAmount(GetNodeCost(), sellReturnPercent);

        if (connectedBuildings.Count > 0)
            ToggleResidual(true);
        else
            Destroy(gameObject);
    }

    public override void ShowDefaultLines()
    {
        ResetLines();
        RemoveNullBuildings();

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
        RemoveNullBuildings();

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
        RemoveNullBuildings();

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

    private void RemoveNullBuildings()
    {
        int i = 0;
        while (i < connectedBuildings.Count)
        {
            if (connectedBuildings[i] != null) i++;
            else connectedBuildings.RemoveAt(i);
        }
    }
}