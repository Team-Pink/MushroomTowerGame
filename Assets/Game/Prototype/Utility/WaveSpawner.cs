using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyTypes
{
    public List<GameObject> enemyPrefabs;
    [SerializeField] List<float> chanceToAppear;

    public float GetChance(GameObject prefab)
    {
        int prefabIndex = -1;
        if (enemyPrefabs.Contains(prefab))
            prefabIndex = enemyPrefabs.IndexOf(prefab);

        if (prefabIndex > 0)
            return chanceToAppear[prefabIndex];
        else
        {
            Debug.LogError("The list does not contain a prefab for " + prefab.name, prefab);
            return -1;
        }
    }
}

[System.Serializable]
public class Wave
{
    // \/ Implement this once we have multiple enemy types \/
    //public EnemyTypes enemyTypes;

    public float durationInSeconds;
    public int enemyCount;
}


public class WaveSpawner : MonoBehaviour
{
    public enum State
    {
        BetweenWaves,
        Spawning,
        WaitingForWaveEnd
    }

    [SerializeField] Path[] paths;
    private Path currentPath;
    private Transform currentSpawnPoint;

    [SerializeField] Wave[] waves;
    private Wave currentWave;
    private int currentWaveIndex;

    private State spawnState;
    private float spawnCooldown;
    private float cooldownElapsed;

    [SerializeField] float secondsBetweenWaves = 5.0f;
    private float elapsedSecondsBetweenWaves;

    [SerializeField] GameObject enemyPrefab;
    private int spawnedEnemies;
    private List<Enemy> aliveEnemies = new();

    private void Awake()
    {
        currentPath = paths[Random.Range(0, paths.Length - 1)];

        currentWave = SpawnWave(currentWaveIndex);

        spawnCooldown = currentWave.durationInSeconds / currentWave.enemyCount;
    }

    private void Update()
    {
        switch (spawnState)
        {
            case State.BetweenWaves:
                BetweenWavesState();
                break;
            case State.Spawning:
                SpawningState();
                break;
            case State.WaitingForWaveEnd:
                WaitingForWaveEndState();
                break;
        }

        List<Enemy> enemiesToRemove = new();
        foreach (Enemy enemy in aliveEnemies)
        {
            if (enemy.dead)
            {
                enemiesToRemove.Add(enemy);
            }
        }
        foreach (Enemy enemy in enemiesToRemove)
        {
            aliveEnemies.Remove(enemy);
        }
    }

    private void BetweenWavesState()
    {
        if (elapsedSecondsBetweenWaves >= secondsBetweenWaves)
        {
            spawnState = State.Spawning;
            currentWave = SpawnWave(currentWaveIndex);
            elapsedSecondsBetweenWaves = 0.0f;
        }
        else
            elapsedSecondsBetweenWaves += Time.deltaTime;
    }

    private void SpawningState()
    {
        if (cooldownElapsed >= spawnCooldown)
        {
            Enemy enemy = SpawnEnemy().GetComponent<Enemy>();
            enemy.GetComponent<Enemy>().pathToFollow = currentPath;
            enemy.gameObject.SetActive(true);
            aliveEnemies.Add(enemy);

            cooldownElapsed = 0.0f;

            spawnedEnemies++;

            if (spawnedEnemies >= currentWave.enemyCount)
                spawnState = State.WaitingForWaveEnd;
        }
        else
        {
            cooldownElapsed += Time.deltaTime;
        }
    }

    private void WaitingForWaveEndState()
    {
        if (aliveEnemies.Count == 0)
        {
            if (currentWaveIndex + 1 < waves.Length)
            {
                Debug.Log("Next Wave Starting");
                InitialiseNextWave();
            }
            else
                Debug.Log("Final Wave Defeated");
        }
    }

    private void InitialiseNextWave()
    {
        spawnState = State.BetweenWaves;

        currentWaveIndex++;
        currentWave = SpawnWave(currentWaveIndex);

        currentPath = paths[Random.Range(0, paths.Length - 1)];

        spawnedEnemies = 0;
        spawnCooldown = currentWave.durationInSeconds / currentWave.enemyCount;
    }

    private Wave SpawnWave(int waveIndex)
    {
        Path currentPath = paths[Random.Range(0, paths.Length - 1)];
        currentSpawnPoint = currentPath.transform.GetChild(0);

        return waves[waveIndex];
    }

    private GameObject SpawnEnemy()
    {
        // \/ Implement this once we have multiple enemy types \/
        //GameObject[] enemyPool = currentWave.enemyTypes.enemyPrefabs.ToArray();
        //GameObject prefabToSpawn = enemyPool[Random.Range(0, enemyPool.Length - 1)];

        GameObject enemyObject = Instantiate(enemyPrefab, currentSpawnPoint.position, Quaternion.identity);

        return enemyObject;
    }
}