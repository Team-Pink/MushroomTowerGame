using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitScript : MonoBehaviour
{
    Button button;

    private void Start()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(Quit);
    }
    
    // get rid of this script when you have an actual dedicated quit or pause UI manager.
    private void Quit()
    {
        Application.Quit();
    }
}
