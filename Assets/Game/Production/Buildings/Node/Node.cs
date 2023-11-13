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
            RemoveNullBuildings();

            int shrooms = 0;

            foreach (var building in connectedBuildings)
            {
                if (building is Shroom)
                    shrooms++;
            }

            return shrooms;
        }
        private set { }
    }
    public int connectedNodesCount
    {
        get
        {
            RemoveNullBuildings();

            int nodes = 0;

            foreach (var building in connectedBuildings)
            {
                if (building is Node)
                    nodes++;
            }

            return nodes;
        }
        private set { }
    }

    private LineMode lineMode = LineMode.Default;
    public GameObject displayLinePrefab;
    public Material displayLineDefault;
    public Material displayLineHighlighted;
    public Material displayLineSell;
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
        RemoveNullBuildings();

        if (isResidual && connectedNodesCount == 0 && connectedShroomsCount == 0)
            Destroy(gameObject);

        bool showBud = !budDetached && !isResidual && Active;
        bud.SetActive(showBud);

        if (lineMode == LineMode.Default)
        {
            recurseHighlight = false;
            for (int i = 0; i < connectedBuildings.Count; i++)
            {
                if (connectedBuildings[i].recurseHighlight)
                {
                    recurseHighlight = true;
                    SetLineHighlighted(connectedBuildings[i]);
                }
                else if (connectedBuildings[i].showSelling)
                {
                    SetLineSell(connectedBuildings[i]);
                }
                else
                {
                    SetLineDefault(connectedBuildings[i]);
                }
            }
        }
    }

    public void AddBuilding(Building building)
    {
        connectedBuildings.Add(building);

        AddLine(building);
    }
    public void RemoveBuilding(int index)
    {
        RemoveLine(index);

        connectedBuildings.RemoveAt(index);
    }

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
            if (cursorManager.currentCursorState != "CanBuy")
                cursorManager.ChangeCursor("CanBuy");
        else
            if (cursorManager.currentCursorState != "CannotBuy")
            cursorManager.ChangeCursor("CannotBuy");
    }
    public void SetCursorCostToNull()
    {
        CursorManager cm = GameObject.Find("GameManager").GetComponent<CursorManager>();
        cm.DisplayCost();
        if (cm.currentCursorState != "Default")
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

    public override void AddLine(Building target)
    {
        GameObject line = Instantiate(displayLinePrefab, transform);
        displayLines.Add(line);

        LineRenderer renderer = line.GetComponent<LineRenderer>();
        renderer.material = displayLineDefault;
        renderer.SetPosition(0, target.transform.position - transform.position + lineRendererOffset);
        renderer.SetPosition(1, lineRendererOffset);
    }
    public override void RemoveLine(Building target)
    {
        int index = connectedBuildings.IndexOf(target);
        Destroy(displayLines[index]);
        displayLines.Remove(displayLines[index]);
    }
    public void RemoveLine(int index)
    {
        Destroy(displayLines[index]);
        displayLines.Remove(displayLines[index]);
    }

    public override void SetLineDefault(Building target)
    {
        int index = connectedBuildings.IndexOf(target);
        displayLines[index].GetComponent<LineRenderer>().material = displayLineDefault;
    }

    public override void SetLinesDefault()
    {
        lineMode = LineMode.Default;

        foreach (GameObject line in displayLines)
        {
            line.GetComponent<LineRenderer>().material = displayLineDefault;
        }
        foreach (Building building in connectedBuildings)
        {
            if (building is Node)
            {
                building.SetLinesDefault();
            }
        }
    }

    public override void SetLineHighlighted(Building target)
    {
        int index = connectedBuildings.IndexOf(target);
        displayLines[index].GetComponent<LineRenderer>().material = displayLineHighlighted;
    }

    public override void SetLinesHighlighted()
    {
        lineMode = LineMode.Highlighted;

        foreach (GameObject line in displayLines)
        {
            line.GetComponent<LineRenderer>().material = displayLineHighlighted;
        }
        foreach (Building building in connectedBuildings)
        {
            if (building is Node)
            {
                building.SetLinesHighlighted();
            }
        }
    }

    public override void SetLineSell(Building target)
    {
        int index = connectedBuildings.IndexOf(target);
        displayLines[index].GetComponent<LineRenderer>().material = displayLineSell;
    }

    public override void SetLinesSell()
    {
        lineMode = LineMode.Sell;

        foreach (GameObject line in displayLines)
        {
            line.GetComponent<LineRenderer>().material = displayLineSell;
        }
        foreach (Building building in connectedBuildings)
        {
            if (building is Node)
            {
                building.SetLinesSell();
            }
        }
    }

    private void RemoveNullBuildings()
    {
        int i = 0;
        while (i < connectedBuildings.Count)
        {
            if (connectedBuildings[i] != null) i++;
            else RemoveBuilding(i);
        }
    }
}