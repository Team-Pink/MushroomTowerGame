using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.SceneManagement.SceneManager;
using Text = TMPro.TMP_Text;

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
    public List<GameObject> enemyPrefabs;
    // \/ Swap for this when implementing different chances for enemies \/
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
    private readonly List<Enemy> aliveEnemies = new();

    [SerializeField] Hub hub;
    [SerializeField] Text wonText;

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
            if (enemy.isDead)
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
            enemy.pathToFollow = currentPath;
            enemy.hub = hub;
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
            {
                wonText.enabled = true;

                Debug.Log("Final Wave Defeated");

                StartCoroutine(GameWon());
            }

        }
    }

    private IEnumerator GameWon()
    {
        yield return new WaitForSeconds(5);
        LoadScene(GetActiveScene().buildIndex);
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

    int enemyNumber = 0;
    private GameObject SpawnEnemy()
    {
        GameObject[] enemyPool = currentWave.enemyPrefabs.ToArray();
        // \/ Swap for this when implementing different chances for enemies \/
        //GameObject[] enemyPool = currentWave.enemyTypes.enemyPrefabs.ToArray();
        GameObject prefabToSpawn = enemyPool[Random.Range(0, enemyPool.Length - 1)];

        GameObject enemyObject = Instantiate(prefabToSpawn, currentSpawnPoint.position, Quaternion.identity, GameObject.Find("----|| Enemies ||----").transform);

        enemyObject.name = "Enemy " + enemyNumber;
        enemyNumber++;

        return enemyObject;
    }
}