using PylonList = System.Collections.Generic.List<Pylon>;
using Text = TMPro.TMP_Text;
using UnityEngine;

using static UnityEngine.Time;
using static UnityEngine.SceneManagement.SceneManager;

public class Hub : Building
{
    [SerializeField] float maxHealth = 100;
    private float currentHealth;
    [SerializeField] float gameOverDuration = 10;
    [SerializeField] Text gameOverText;
    [SerializeField] Text hubHealthText;

    public MeshRenderer healthDisplay;
    public float healthDisplayWindow = 2.0f;
    private float healthDisplayActiveTime;

    
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

        if (healthDisplayActiveTime < healthDisplayWindow)
        {
            healthDisplayActiveTime += deltaTime;

            if (healthDisplayActiveTime >= healthDisplayWindow)
            {
                healthDisplay.enabled = false;
            }
        }

        hubHealthText.text = currentHealth.ToString();

        ClearDestroyedPylons();
        connectedPylonsCount = pylonCount;
    }

    public void Damage(float damageAmount)
    {
        currentHealth -= damageAmount;
        healthDisplay.sharedMaterial.SetFloat("_Value", currentHealth / maxHealth);
        healthDisplay.enabled = true;
        healthDisplayActiveTime = 0;
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
}