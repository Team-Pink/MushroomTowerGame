using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
struct Page
{
    public Image instructionPanel;
    [TextArea] public  string instructionText;
}

public class InstructionUI : MonoBehaviour
{
    [SerializeField] Page[] instructionPages;

    [SerializeField] Button openInstButton;
    [SerializeField] GameObject instructionPanel;

    //private variables
    int pageNumber = 0;

    private void Start()
    {
        OpenInst();
    }


    public void OpenInst()
    {
        instructionPanel.SetActive(true);
        openInstButton.interactable = false;
    }

    public void CloseInst()
    {
        instructionPanel.SetActive(false);
        openInstButton.interactable = true;
        pageNumber = 0;
    }
}
