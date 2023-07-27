using GameObjectList = System.Collections.Generic.List<UnityEngine.GameObject>;
using UnityEngine;

public class BoidCreator : MonoBehaviour
{
    [SerializeField] int amount;
    [SerializeField] GameObject boidPrefab;

    private GameObjectList boidList = new();

    private new Transform transform;

    private void Awake()
    {
        transform = GetComponent<Transform>();
    }

    private void Update()
    {
        if (boidList.Count < 100)
        {
            SpawnBoids();
        }
    }

    [ContextMenu("Spawn Boids")]
    public void SpawnBoids()
    {
        for (int boidIndex = 0; boidIndex < amount; boidIndex++)
        {
            Quaternion rotation = new(0, Random.Range(-1.0f, 1.0f), 0.0f, 0.0f);
            Vector3 position = transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f));

            boidList.Add(Instantiate(boidPrefab, position, rotation, transform));
        }

        foreach (GameObject boid in boidList)
        {
            BoidLogic boidLogic = boid.GetComponent<BoidLogic>();
            boidLogic.boidList = boidList;
        }
    }
}