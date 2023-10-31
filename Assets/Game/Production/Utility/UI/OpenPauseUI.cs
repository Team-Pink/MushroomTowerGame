using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class OpenPauseUI : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    bool activePause = false;

    void Start()
    {
    }

    // Update is called once per frame
    public void TogglePauseMenu()
    {
        Time.timeScale = (Time.timeScale == 0) ? 1 : 0;

        activePause = !activePause;
        // pause game
        pauseMenu.SetActive(activePause);
    }

    public void Quit()
    {
        Debug.Log("Quit button has ben pressed but no unfortunately you can't quit unity.");
        Debug.Log("I meant the editor...");
#if UNITY_EDITOR
        EditorApplication.Exit(0);
        #else
                    Application.Quit();
#endif
    }
}
