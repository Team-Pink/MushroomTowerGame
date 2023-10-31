using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject hudIcons;
    [SerializeField] GameObject tutorial;

    [SerializeField] GameObject mainMenu;

    public void Play()
    {
        hudIcons.SetActive(true);
        tutorial.SetActive(true);

        mainMenu.SetActive(false);
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