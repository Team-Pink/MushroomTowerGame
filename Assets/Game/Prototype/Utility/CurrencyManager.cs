using System.Collections;
using System.Collections.Generic;
using Text = TMPro.TMP_Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] Text currencyText;
    [SerializeField] int startingAmount = 20;
    int currencyAmount = 0;

    private void Start()
    {
        currencyAmount = startingAmount;
        currencyText.text = currencyAmount.ToString();
    }

    public void IncreaseCurrencyAmount(int amount)
    {
        currencyAmount += amount;
        currencyText.text = currencyAmount.ToString();
    }

    public bool DecreaseCurrencyAmount(int amount)
    {
        if(currencyAmount < amount)
        {
            Debug.LogWarning("Cannot remove amount from currency total");
            return false;
        }

        currencyAmount -= amount;
        currencyText.text = currencyAmount.ToString();
        return true;
    }

    public int GetCurrencyTotal()
    {
        return currencyAmount;
    }

    private void Update()
    {
    }
}
