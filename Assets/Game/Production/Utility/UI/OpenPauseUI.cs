using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenPauseUI : MonoBehaviour
{
    public GameObject pauseMenu;
    Button button;
    bool activePause = false;

    void Start()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(OpenPauseMenu);
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
}
