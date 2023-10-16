using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FpsCounter : MonoBehaviour
{
    TMP_Text text;
    [SerializeField] float timer = 0;
    [SerializeField] float countedFrames = 0;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (timer >= 1)
        {
            text.text = (countedFrames / timer).ToString("F2");
            timer = 0;
            countedFrames = 0;
        }
        else
        {
            timer += Time.deltaTime;
            countedFrames++;
        }
    }
}
