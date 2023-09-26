using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.SceneManagement.SceneManager;
using Text = TMPro.TMP_Text;

[System.Serializable]
public class Wave
{
    public List<GameObject> enemyPrefabs;

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

    [SerializeField] Transform[] spawnPoints;
    private Transform currentSpawnPoint;
    private int currentSpawnPointIndex;

    [SerializeField] Wave[] waves;
    private Wave currentWave;
    private int currentWaveIndex;

    private State spawnState = State.BetweenWaves;
    private float spawnCooldown;
    private float cooldownElapsed;

    [SerializeField] float secondsBetweenWaves = 5.0f;
    private float elapsedSecondsBetweenWaves;

    private int spawnedEnemies;
    private readonly List<Enemy> aliveEnemies = new();

    [SerializeField] Transform parentFolder;
    [SerializeField] Hub hub;
    [SerializeField] Text wonText;
    private LevelDataGrid levelData;

    WaveCounter waveCounterUI; // UI element that needs to be updated at the end of a wave.
    
    [SerializeField] RectTransform waveIndicator;
    [SerializeField] Image waveTimer;
    [SerializeField] Vector2[] indicatorPositions;

    private void Awake()
    {
        levelData = GetComponent<LevelDataGrid>();

        CalculateSpawns();

        currentWave = waves[currentWaveIndex];
        waveIndicator.position = indicatorPositions[currentSpawnPointIndex];
        waveIndicator.gameObject.SetActive(true);

        spawnCooldown = currentWave.durationInSeconds / currentWave.enemyCount;

        waveCounterUI = FindObjectOfType<WaveCounter>(); // I hope this works ;)
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
            if (enemy.Dead)
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
            elapsedSecondsBetweenWaves = 0.0f;

            waveIndicator.gameObject.SetActive(false);
        }
        else
        {
            waveTimer.fillAmount = 1 - (elapsedSecondsBetweenWaves / secondsBetweenWaves);
            elapsedSecondsBetweenWaves += Time.deltaTime;
        }
    }

    private void SpawningState()
    {
        if (cooldownElapsed >= spawnCooldown)
        {
            Enemy enemy = SpawnEnemy().GetComponent<Enemy>();
            enemy.hub = hub;
            enemy.hubTransform = hub.transform;
            enemy.levelData = levelData;
            enemy.transform.gameObject.SetActive(true);
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
            UpdateWaveCounterUI();
            if (currentWaveIndex + 1 < waves.Length)
            {

                Debug.Log("Next Wave Starting in " + secondsBetweenWaves + " Seconds");

                InitialiseNextWave();

                waveIndicator.position = indicatorPositions[currentSpawnPointIndex];
                waveIndicator.gameObject.SetActive(true);
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
        CalculateSpawns();
        currentWave = waves[currentWaveIndex];

        spawnedEnemies = 0;
        spawnCooldown = currentWave.durationInSeconds / currentWave.enemyCount;
    }

    private void CalculateSpawns()
    {
        currentSpawnPointIndex = Random.Range(0, spawnPoints.Length - 1);
        currentSpawnPoint = spawnPoints[currentSpawnPointIndex];
    }

    int enemyNumber = 0;
    private GameObject SpawnEnemy()
    {
        GameObject[] enemyPool = currentWave.enemyPrefabs.ToArray();
        GameObject prefabToSpawn = enemyPool[Random.Range(0, enemyPool.Length)];

        GameObject enemyObject = Instantiate(prefabToSpawn, currentSpawnPoint.position, Quaternion.identity, parentFolder);

        enemyObject.name = "Enemy " + enemyNumber;
        enemyNumber++;

        return enemyObject;
    }

    /// <summary>
    /// This is Lochlan's code for updating the WaveCounter UI element
    /// </summary>
    private void UpdateWaveCounterUI()
    {
        float waveProgress = (float)(currentWaveIndex +1)/ (float)waves.Length;
        waveCounterUI.AnimateBitsFalling();
        waveCounterUI.SetWaveCounterFill(waveProgress);
    }
}