using UnityEngine;
using UnityEngine.UI;

public class Spawnpoint : MonoBehaviour
{
    public GameObject waveIndicator;
    public Image timer;

    public Vector3 position
    {
        get
        {
            return transform.position;
        }
    }
}