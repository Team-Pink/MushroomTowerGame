using System.Collections;
using System.Collections.Generic;
using Text = TMPro.TMP_Text;
using UnityEngine;
using UnityEditor;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] Text currencyText;
    int currencyAmount = 0;

    private void Start()
    {
        currencyText.text = currencyAmount.ToString();
    }

    public void IncreaseCurrencyAmount(int amount)
    {
        currencyAmount += amount;
        currencyText.text = currencyAmount.ToString();
    }

    public void DecreaseCurrencyAmount(int amount)
    {
        if(currencyAmount < amount)
        {
            Debug.LogWarning("Cannot remove amount from currency total");
#if UNITY_EDITOR
            EditorApplication.Beep();
#endif
            return;
        }

        currencyAmount -= amount;
        currencyText.text = currencyAmount.ToString();
    }

    public int GetCurrencyTotal()
    {
        return currencyAmount;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            IncreaseCurrencyAmount(1);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            DecreaseCurrencyAmount(1);
        }
    }
}
