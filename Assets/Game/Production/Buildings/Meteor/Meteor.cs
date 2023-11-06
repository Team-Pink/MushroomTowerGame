using System.Collections.Generic;
using Text = TMPro.TMP_Text;
using UnityEngine;

using static UnityEngine.Time;
using static UnityEngine.SceneManagement.SceneManager;

public class Meteor : Building
{
    [SerializeField] float maxHealth = 100;
    private float currentHealth;
    [SerializeField] float gameOverDuration = 10;
    [SerializeField] Text gameOverText;

    public MeshRenderer healthDisplay;

    [HideInInspector] public bool budDetached = false;
    
    public List<Node> connectedNodes;

    public GameObject displayLinePrefab;
    public Material displayLineMaterial;
    private List<GameObject> displayLines = new();
    private Vector3 lineRendererOffset = new(0, 1.5f, 0);

    private void Awake()
    {
        currentHealth = maxHealth;
        healthDisplay.sharedMaterial.SetFloat("_Value", currentHealth / maxHealth);
    }

    private void Update()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            gameOverText.enabled = true;

            if (gameOverDuration > 0)
                gameOverDuration -= deltaTime;
            else
                RestartScene();
        }

        ClearDestroyedNodes();

        bud.SetActive(!budDetached);
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
    }

    public void RemoveNode(Node node)
    {
        connectedNodes.Remove(node);
    }

    public override void ShowDefaultLines()
    {
        ResetLines();

        for (int i = 0; i < connectedNodes.Count; i++)
        {
            Building building = connectedNodes[i];

            GameObject line = Instantiate(displayLinePrefab, transform);
            displayLines.Add(line);

            LineRenderer renderer = line.GetComponent<LineRenderer>();
            renderer.material = displayLineMaterial;
            renderer.SetPosition(0, (transform.position - building.transform.position) + lineRendererOffset);
            renderer.SetPosition(1, lineRendererOffset);
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
    }
}