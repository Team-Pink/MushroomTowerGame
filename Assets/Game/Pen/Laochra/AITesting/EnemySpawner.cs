using BoidList = System.Collections.Generic.List<BoidReference>;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] int amount;
    [SerializeField] GameObject boidPrefab;

    private readonly BoidList boidList = new();

    private new Transform transform;
    private LevelDataGrid levelData;
    public Transform hubTransform;

    private void Awake()
    {
        transform = GetComponent<Transform>();
        levelData = GetComponent<LevelDataGrid>();

        SpawnBoids();
    }

    [ContextMenu("Spawn Boids")]
    public void SpawnBoids()
    {
        for (int boidIndex = 0; boidIndex < amount; boidIndex++)
        {
            Quaternion rotation = new(0, Random.Range(-1.0f, 1.0f), 0.0f, 0.0f);
            Vector3 position = transform.position + new Vector3(Random.Range(-60.0f, 60.0f), 0.0f, Random.Range(-60.0f, 60.0f));

            GameObject boidGameObject = Instantiate(boidPrefab, position, rotation, transform);
            Transform boidTransform = boidGameObject.transform;
            Rigidbody boidRigidbody = boidGameObject.GetComponent<Rigidbody>();
            EnemyLogic boidLogic = boidGameObject.GetComponent<EnemyLogic>();

            boidLogic.levelData = levelData;
            boidLogic.hubTransform = hubTransform;

            boidList.Add(new BoidReference(boidTransform, boidRigidbody, boidLogic));
        }
    }
}