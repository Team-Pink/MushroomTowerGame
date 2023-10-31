using PylonList = System.Collections.Generic.List<Pylon>;
using Text = TMPro.TMP_Text;
using UnityEngine;

using static UnityEngine.Time;
using static UnityEngine.SceneManagement.SceneManager;
using System.Collections.Generic;

public class Hub : Building
{
    [SerializeField] float maxHealth = 100;
    private float currentHealth;
    [SerializeField] float gameOverDuration = 10;
    [SerializeField] Text gameOverText;

    public MeshRenderer healthDisplay;

    [HideInInspector] public bool budDetached = false;
    
    public PylonList connectedPylons;
    public int pylonCount
    {
        get
        {
            return connectedPylons.Count;
        }
        private set { }
    }
    public int connectedPylonsCount = 0;
    private bool atMaxPylons;
    [HideInInspector] public bool AtMaxPylons { get => pylonCount == InteractionManager.hubMaxPylons; }

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

        ClearDestroyedPylons();
        connectedPylonsCount = pylonCount;

        if (atMaxPylons != AtMaxPylons)
        {
            radiusDisplay.transform.GetChild(0).gameObject.SetActive(!AtMaxPylons);
            radiusDisplay.transform.GetChild(1).gameObject.SetActive(!AtMaxPylons);

            atMaxPylons = AtMaxPylons;
        }

        bool showBud = !(AtMaxPylons || budDetached);
        bud.SetActive(showBud);
    }

    public void Damage(float damageAmount)
    {
        currentHealth -= damageAmount;
        healthDisplay.sharedMaterial.SetFloat("_Value", currentHealth / maxHealth);
        if (!healthDisplay.enabled) healthDisplay.enabled = true;
    }

    private void RestartScene() => LoadScene(GetActiveScene().name);

    public void ClearDestroyedPylons()
    {
        for (int i = 0; i < connectedPylons.Count; i++)
        {
            Pylon pylon = connectedPylons[i];
            if (pylon == null)
                RemovePylon(pylon);
        }
    }

    public void AddPylon(Pylon pylon)
    {
        connectedPylons.Add(pylon);
    }

    public void RemovePylon(Pylon pylon)
    {
        connectedPylons.Remove(pylon);
    }

    public override void ShowDefaultLines()
    {
        ResetLines();

        for (int i = 0; i < connectedPylons.Count; i++)
        {
            Building building = connectedPylons[i];

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