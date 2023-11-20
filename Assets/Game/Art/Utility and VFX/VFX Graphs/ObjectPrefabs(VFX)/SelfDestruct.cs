using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SelfDestruct : MonoBehaviour
{
    public float secondsTillDestruction;
    private float detonationTime;

    private void Start()
    {
        detonationTime = Time.time + secondsTillDestruction;
        GetComponent<VisualEffect>().Play();
    }
    // Update is called once per frame
    void Update()
    {
        // I don't understand coroutines or the timer class well enough to make this more efficient using them.
        if (Time.time > detonationTime)
        {
            Destroy(gameObject);
        }
    }
}
