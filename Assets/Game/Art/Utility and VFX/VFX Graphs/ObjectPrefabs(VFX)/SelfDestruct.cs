using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SelfDestruct : MonoBehaviour
{
    public float secondsTillDestruction;
<<<<<<< Updated upstream
    private float detonationTime;
    private VisualEffect VFX;

    private void Start()
    {
        detonationTime = Time.time + secondsTillDestruction;
        VFX = GetComponent<VisualEffect>();
        VFX.Play();
      //  Destroy(gameObject, 10.0f);
    }

    void Update()
    {
        // I don't understand the timer class well enough to make this more efficient using it, for the moment.
        if (Time.time > detonationTime)
        {
            Destroy(gameObject); // VFX.spawnParticleRate = Vector3.Zero;
            //VFX.SetFloat("particleSpawnRate", 0.0f);

            if (VFX.aliveParticleCount <= 0) Destroy(gameObject);
        }

=======

    private void Start()
    {
        GetComponent<VisualEffect>().Play();
        Destroy(gameObject,secondsTillDestruction);
        
>>>>>>> Stashed changes
    }
    // I prototyped a coroutine but due to the double condition in a loop it just isn't worth it.
}
