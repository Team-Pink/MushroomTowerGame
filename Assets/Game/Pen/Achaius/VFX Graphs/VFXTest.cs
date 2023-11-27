using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXTest : MonoBehaviour
{
    [SerializeField] VisualEffect codingWithConnor;
    public float particleSpawnRate;
    public Vector2 particleLifetime;

    void Start()
    {
        codingWithConnor = GetComponent<VisualEffect>();

    }

    // Update is called once per frame
    void Update()
    {
        codingWithConnor.SetFloat("SpawnRate", particleSpawnRate);
        codingWithConnor.SetVector2("lfetime", particleLifetime);
    }
}
