using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionUI : MonoBehaviour
{

    [SerializeField] Button openInstButton;
    [SerializeField] GameObject instructionPanel;

    private void Start()
    {
        OpenInst();
    }


    public void OpenInst()
    {
        Time.timeScale = 0;
        instructionPanel.SetActive(true);
        openInstButton.interactable = false;
    }

    public void CloseInst()
    {
        Time.timeScale = 1;
        instructionPanel.SetActive(false);
        openInstButton.interactable = true;
    }
}
