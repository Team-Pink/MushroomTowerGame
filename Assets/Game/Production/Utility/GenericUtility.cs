using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public static class GenericUtility
{
    public static void DestroyAllChildren(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }
    }

    public static float CalculateTime(float velocity, float distance)
    {
        return distance/velocity;
    }

    public static float CalculateVelocity(float distance, float time)
    {
        return distance / time;
    }

    public static float CalculateDistance(float velocity, float time)
    {
        return time * velocity;
    }
    public static float CalculateDistance(Vector3 posA, Vector3 posB)
    {
        return Mathf.Sqrt(Mathf.Pow(posB.x - posA.x, 2) + Mathf.Pow(posB.z - posA.z, 2));
    }
}
