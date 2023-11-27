using System.Collections.Generic;
using UnityEngine;

public class Node : Building
{
    public Animator animator;
    public Animator budAnimator;

    public MeshRenderer healthDisplay;
    public override bool IsMaxHealth
    {
        get => CurrentHealth == MaxHealth;
    }
    public bool isResidual
    {
        get;
        private set;
    }

    [HideInInspector] public bool budDetached = false;

    [Header("Purchasing and Selling")]
    [SerializeField] int costMultiplier = 1;
    [SerializeField, Range(0, 1)] float sellReturnPercent = 0.5f;
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
            if (currentHealth <= float.Epsilon && !isResidual)
            {
                AudioManager.PlaySoundEffect(deathAudio.name, 1);
                if (CanTurnIntoResidual())
                    ToggleResidual(true);
            }
        }
    }

    [SerializeField] public bool disappearing = false;

    public GameObject regrowCanvas;
    UnityEngine.UI.Button regrowButton;

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
        healthDisplay.sharedMaterial.SetFloat("_Value", currentHealth / MaxHealth);
        AudioManager.PlaySoundEffect(placeAudio.name, 1);
        regrowButton = regrowCanvas.GetComponentInChildren<UnityEngine.UI.Button>();
    }

    private void Update()
    {
        if (InteractionManager.gamePaused && regrowButton.interactable) regrowButton.interactable = false;
        else if (!InteractionManager.gamePaused && !regrowButton.interactable) regrowButton.interactable = true;

        RemoveNullBuildings();

        if (isResidual && connectedNodesCount == 0 && connectedShroomsCount == 0 && !disappearing)
        {
            budAnimator.SetBool("Sell", true);
            animator.SetBool("Residual Disappear", true);
            disappearing = true;
        }

        bool showBud = !budDetached;
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

    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        if (healthDisplay != null)
        {
            healthDisplay.sharedMaterial.SetFloat("_Value", currentHealth / MaxHealth);
            if (!healthDisplay.enabled) healthDisplay.enabled = true;
        }
    }

    public void AddBuilding(Building building)
    {
        connectedBuildings.Add(building);

        if (!Active || isResidual)
        {
            building.Deactivate();
        }

        AddLine(building);
    }
    public void RemoveBuilding(int index)
    {
        RemoveLine(index);

        connectedBuildings.RemoveAt(index);
    }

    public override void Deactivate()
    {
        if (!Active) return;
        base.Deactivate();
        foreach (Building building in connectedBuildings)
        {
            building.Deactivate();
        }
        budAnimator.SetBool("Deactivate", true);
    }
    public override void Reactivate()
    {
        if (Active) return;
        base.Reactivate();
        foreach (Building building in connectedBuildings)
        {
            if (building is Node && (building as Node).isResidual) continue;

            building.Reactivate();
        }
        budAnimator.SetBool("Reactivate", true);

        currentHealth = nodeHealth;
    }

    public void ToggleResidual(bool value)
    {
        isResidual = value;

        if (isResidual)
        {
            Deactivate();
        }
        else
        {
            Reactivate();
        }

        if (Active)
        {
            animator.SetBool("Rebuild", true);
        }
        else
        {
            animator.SetBool("Become Residual", true);
        }

        regrowCanvas.SetActive(isResidual);
    }
    public bool CanTurnIntoResidual()
    {
        RemoveNullBuildings();

        if (connectedBuildings.Count > 0)
            return true;
        else
        {
            if (budAnimator != null) budAnimator.SetBool("Sell", true);
            if (animator != null) animator.SetBool("Residual Disappear", true);
        }
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
        if (InteractionManager.gamePaused) return;

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
        {
            disappearing = true;
            budAnimator.SetBool("Sell", true);
            animator.SetBool("Sell", true);
        }
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
    
    /// <summary>
    /// To hook up to animations so the node gets destroyed at the end of the animation.
    /// </summary>
    private void DestroyNode()
    {
        Destroy(gameObject);
    } 
}