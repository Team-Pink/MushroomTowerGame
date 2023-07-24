using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class funny_Haha : MonoBehaviour
{
    public bool funny = false;
    public bool funnier = false;
    public bool Funniest;

    // Start is called before the first frame update
    void Awake()
    {
        if (funnier)
        {
            EditorApplication.Exit(0);
        }

        if (funny)
        {
            Debug.Log("No Game 4 U");
            EditorApplication.ExitPlaymode();
        }

        StartCoroutine(FunnyPlaying());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator FunnyPlaying()
    {
        while (EditorApplication.isPlaying)
        {
            EditorApplication.Beep();
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }
}
