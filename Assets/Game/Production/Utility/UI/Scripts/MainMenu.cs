using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject hudIcons;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject boomy;
    [SerializeField] GameObject creditsMenu;

    private TutorialManager tutorialManager;

    private void Awake()
    {
        tutorialManager = GameObject.Find("GameManager").GetComponent<TutorialManager>();
        InteractionManager.gamePaused = true;
    }

    public void GoCredits() {
        mainMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void GoMainMenu()
    {
        mainMenu.SetActive(true);
        creditsMenu.SetActive(false);
    }

    public void Play()
    {
        hudIcons.SetActive(true);
        mainMenu.SetActive(false);
        boomy.SetActive(true);

        tutorialManager.StartTutorial(TutorialManager.Tutorial.Placement);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}