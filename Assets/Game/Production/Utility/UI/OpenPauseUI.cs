using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenPauseUI : MonoBehaviour
{
    private GameObject pauseMenu;
    Button pausebutton;
    Button quitButton;
    Button resumebutton;
    bool activePause = false;

    void Start()
    {
        pauseMenu = transform.GetChild(0).gameObject;
        pausebutton = this.GetComponent<Button>();
        pausebutton.onClick.AddListener(OpenPauseMenu);
        quitButton = pauseMenu.transform.GetChild(0).GetComponent<Button>();
        quitButton.onClick.AddListener(Quit);
        resumebutton = pauseMenu.transform.GetChild(1).GetComponent<Button>();
        resumebutton.onClick.AddListener(OpenPauseMenu);
    }

    // Update is called once per frame
    private void OpenPauseMenu()
    {
        activePause = !activePause;
        Debug.LogWarning("No Pause Menu Exists!");
        Debug.LogWarning("Opening pause menu does not pause gameplay!");
        // pause game
        pauseMenu.SetActive(activePause);
    }

    private void Quit()
    {
        Debug.Log("Quit button has ben pressed but no unfortunately you can't quit unity.");
        Debug.Log("I meant the editor...");
        Application.Quit();
    }
}
