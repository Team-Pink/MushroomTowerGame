using PylonList = System.Collections.Generic.List<Pylon>;
using Text = TMPro.TMP_Text;
using UnityEngine;

using static UnityEngine.Time;
using static UnityEngine.SceneManagement.SceneManager;

public class Hub : Building
{
    [SerializeField] int health = 10;
    [SerializeField] float gameOverDuration = 10;
    [SerializeField] Text gameOverText;
    [SerializeField] Text hubHealthText;

    
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

    private void Update()
    {
        if (health <= 0)
        {
            health = 0;
            gameOverText.enabled = true;

            if (gameOverDuration > 0)
                gameOverDuration -= deltaTime;
            else
                RestartScene();
        }

        hubHealthText.text = health.ToString();

        ClearDestroyedPylons();
        connectedPylonsCount = pylonCount;
    }

    public void Damage(int damageAmount) => health -= damageAmount;

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