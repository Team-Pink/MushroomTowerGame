using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenPauseUI : MonoBehaviour
{
    public GameObject pauseMenu;
    Button button;

    void Start()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(OpenPauseMenu);
    }

    // Update is called once per frame
    private void OpenPauseMenu()
    {
        Debug.LogWarning("No Pause Menu Exists");
        // pause game
        pauseMenu.SetActive(true);
    }

    private void ClosePauseMenu()
    {
        Debug.LogWarning("No Pause Menu Exists");
        // unpause game
        pauseMenu.SetActive(false);
    }
}
