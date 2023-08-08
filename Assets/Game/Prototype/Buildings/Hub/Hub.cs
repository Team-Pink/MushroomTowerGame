using Text = TMPro.TMP_Text;
using SerializeField = UnityEngine.SerializeField;
using HideInInspector = UnityEngine.HideInInspector;
using static UnityEngine.Time;
using static UnityEngine.Input;
using static UnityEngine.Application;
using static UnityEngine.SceneManagement.SceneManager;
using KeyCode = UnityEngine.KeyCode;

public class Hub : Building
{
    [SerializeField] int health = 10;
    [SerializeField] float gameOverDuration = 10;
    [SerializeField] Text gameOverText;
    [SerializeField] Text hubHealthText;

    [HideInInspector] 
    public int pylonCount = 0;

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

        if (GetKey(KeyCode.Escape))
        {
            Quit();
        }
    }

    public void Damage(int damageAmount) => health -= damageAmount;

    private void RestartScene() => LoadScene(GetActiveScene().name);
}