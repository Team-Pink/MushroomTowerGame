using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject Enemy;
    public float frequency = 6;
    float clock=0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (clock > frequency)
        {
            Instantiate(Enemy, RandomPointInBounds(this.GetComponent<BoxCollider>().bounds), Quaternion.identity);
            clock = 0;
        }
        clock += Time.deltaTime;
    }
    
    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            0.5f,
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
