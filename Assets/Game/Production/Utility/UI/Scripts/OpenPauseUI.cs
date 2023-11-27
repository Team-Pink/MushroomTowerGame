using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenPauseUI : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    bool activePause = false;
    Transform buildings;

    void Start()
    {
        buildings = GameObject.Find("----|| Buildings ||----").transform;
    }

    // Update is called once per frame
    public void TogglePauseMenu()
    {
        if (!InteractionManager.tutorialMode)
        {
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;

            InteractionManager.gamePaused = !InteractionManager.gamePaused;


            activePause = !activePause;
            // pause game
            pauseMenu.SetActive(activePause);
            foreach (Building building in buildings.GetComponentsInChildren<Building>())
            {
                if (building == null) continue;
                else if (building is Node && (building as Node).isResidual) { (building as Node).regrowCanvas.SetActive(!activePause); }
            }
        }
    }

    public void Quit()
    {
        Debug.Log("Quit button has ben pressed but It doesn't work in the editor.");
        Application.Quit();

    }
}
