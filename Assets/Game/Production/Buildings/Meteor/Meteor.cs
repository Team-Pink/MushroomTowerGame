using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.Time;
using static UnityEngine.SceneManagement.SceneManager;

public class Meteor : Building
{
    [SerializeField] float maxHealth = 100;
    private float currentHealth;
    public override bool IsMaxHealth
    {
        get => currentHealth == maxHealth;
    }
    [SerializeField] float gameOverDuration = 10;
    [SerializeField] GameObject loseScreen;

    public MeshRenderer healthDisplay;

    [HideInInspector] public bool budDetached = false;
    
    public List<Node> connectedNodes;

    private LineMode lineMode = LineMode.Default;
    public GameObject displayLinePrefab;
    public Material displayLineDefault;
    public Material displayLineHighlighted;
    public Material displayLineSell;
    private List<GameObject> displayLines = new();
    private Vector3 lineRendererOffset = new(0, 1.5f, 0);

    private Animator animator;
    private bool deathTriggerCalled = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        healthDisplay.sharedMaterial.SetFloat("_Value", currentHealth / maxHealth);

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            loseScreen.SetActive(true);

            if (!deathTriggerCalled)
            {
                animator.SetTrigger("Explode");
                deathTriggerCalled = true;
            }

            if (gameOverDuration > 0)
                gameOverDuration -= deltaTime;
            else
                RestartScene();
        }

        ClearDestroyedNodes();

        bud.SetActive(!budDetached);

        if (lineMode == LineMode.Default)
        {
            recurseHighlight = false;
            for (int i = 0; i < connectedNodes.Count; i++)
            {
                if (connectedNodes[i].recurseHighlight)
                {
                    SetLineHighlighted(connectedNodes[i]);
                }
                else if (connectedNodes[i].showSelling)
                {
                    SetLineSell(connectedNodes[i]);
                }
                else
                {
                    SetLineDefault(connectedNodes[i]);
                }
            }
        }
    }

    public void Damage(float damageAmount)
    {
        currentHealth -= damageAmount;
        healthDisplay.sharedMaterial.SetFloat("_Value", currentHealth / maxHealth);
        if (!healthDisplay.enabled) healthDisplay.enabled = true;
    }

    private void RestartScene() => LoadScene(GetActiveScene().name);

    public void ClearDestroyedNodes()
    {
        for (int i = 0; i < connectedNodes.Count; i++)
        {
            Node node = connectedNodes[i];
            if (node == null)
                RemoveNode(node);
        }
    }

    public void AddNode(Node node)
    {
        connectedNodes.Add(node);

        AddLine(node);
    }

    public void RemoveNode(Node node)
    {
        RemoveLine(node);

        connectedNodes.Remove(node);
    }

    public override void AddLine(Building target)
    {
        GameObject line = Instantiate(displayLinePrefab, transform);
        displayLines.Add(line);

        LineRenderer renderer = line.GetComponent<LineRenderer>();
        renderer.material = displayLineDefault;
        renderer.SetPosition(0, (transform.position - target.transform.position) + lineRendererOffset);
        renderer.SetPosition(1, lineRendererOffset);
    }
    public override void RemoveLine(Building target)
    {
        int index = connectedNodes.IndexOf(target as Node);
        Destroy(displayLines[index]);
        displayLines.Remove(displayLines[index]);
    }



    public override void SetLineDefault(Building target)
    {
        int index = connectedNodes.IndexOf(target as Node);
        displayLines[index].GetComponent<LineRenderer>().material = displayLineDefault;
    }

    public override void SetLinesDefault()
    {
        lineMode = LineMode.Default;
        foreach (GameObject line in displayLines)
        {
            line.GetComponent<LineRenderer>().material = displayLineDefault;
        }
        foreach (Node node in connectedNodes)
        {
            node.SetLinesDefault();
        }
    }

    public override void SetLineHighlighted(Building target)
    {
        int index = connectedNodes.IndexOf(target as Node);
        displayLines[index].GetComponent<LineRenderer>().material = displayLineHighlighted;
    }
    public override void SetLinesHighlighted()
    {
        lineMode = LineMode.Highlighted;
        foreach (GameObject line in displayLines)
        {
            line.GetComponent<LineRenderer>().material = displayLineHighlighted;
        }
        foreach (Node node in connectedNodes)
        {
            node.SetLinesHighlighted();
        }
    }

    public override void SetLineSell(Building target)
    {
        int index = connectedNodes.IndexOf(target as Node);
        displayLines[index].GetComponent<LineRenderer>().material = displayLineSell;
    }
}