using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static List<TrapAttacker> trapAttackers = new();

    private void Update()
    {
        while (trapAttackers.Count > 0)
        {
            for (int i = 0; i < trapAttackers.Count; i++)
            {
                trapAttackers[i].PlaceTrap();
            }
        }
    }
}