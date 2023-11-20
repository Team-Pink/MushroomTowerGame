using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// I was going to name this script Bannana Rotate but thought it was a bit too unproffessional...
public class Rotate : MonoBehaviour
{
    /// <summary>
    /// The amount the script adds to the transform per update.
    /// </summary>
    [SerializeField] float RotateAmount;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0 + RotateAmount, 0);
    }
}
