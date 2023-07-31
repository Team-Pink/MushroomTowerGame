using MonoBehaviour = UnityEngine.MonoBehaviour;
using static UnityEngine.SceneManagement.SceneManager;
using Text = TMPro.TMP_Text;
using SerializeField = UnityEngine.SerializeField;
using static UnityEngine.Time;
using static UnityEngine.Input;
using static UnityEngine.Application;
using KeyCode = UnityEngine.KeyCode;

public class Hub : MonoBehaviour
{
    [SerializeField] int health = 10;
    [SerializeField] float gameOverDuration = 10;
    [SerializeField] Text gameOverText;

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

        if (GetKey(KeyCode.Escape))
        {
            Quit();
        }
    }

    public void Damage(int damageAmount) => health -= damageAmount;

    private void RestartScene() => LoadScene(GetActiveScene().name);
}