using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.SceneManagement.SceneManager;
using Text = TMPro.TMP_Text;

[System.Serializable]
public class Batch
{
    public enum EnemyType
    {
        Mastic,
        Biote,
        Dissolvesp,
        Gigant
    }

    [SerializeField] EnemyType enemy = EnemyType.Mastic;
    public GameObject Enemy
    {
        get => WaveSpawner.EnemyPrefabs[(int)enemy];
    }

    [Range(1, 100)] public int amount = 1;

    [Space()]

    [SerializeField] Transform spawnOverride = null;
    public Vector3 spawnPosition
    {
        get
        {
            if (spawnOverride == null)
            {
                Debug.LogError("No spawn point set");
                return Vector3.zero;
            }
            return spawnOverride.position;
        }
    }

    public bool useDefaultSpawn
    {
        get
        {
            return spawnOverride == null;
        }
    }

    [Space()]

    [Tooltip("Play with previous batch")] public bool withPrevious = false;

    [HideInInspector] public int nextEnemy;
}

[System.Serializable]
public class Wave
{
    [Tooltip("Seconds")] public float duration;

    [SerializeField] Transform defaultSpawn = null;
    public Vector3 spawnPosition
    {
        get
        {
            if (defaultSpawn == null)
            {
                Debug.LogError("No wave default spawn point set");
                return Vector3.zero;
            }
            return defaultSpawn.position;
        }
    }

    [Space()]

    public Batch[] batches;

    public int GetTotalEnemyCount()
    {
        int result = 0;

        foreach (Batch batch in batches)
        {
            result += batch.amount;
        }

        return result;
    }
    public int GetGroupedEnemyCount()
    {
        int result = 0;

        int currentMax = 0;
        for (int i = 0; i < batches.Length; i++)
        {
            if (currentMax != 0 && batches[i].withPrevious == false)
            {
                result += currentMax;
                currentMax = 0;
            }

            if (batches[i].amount > currentMax) currentMax = batches[i].amount;
        }

        result += currentMax;

        return result;
    }
}


public class WaveSpawner : MonoBehaviour
{
    public enum State
    {
        BetweenWaves,
        Spawning,
        WaitingForWaveEnd
    }

    [SerializeField] GameObject[] enemyPrefabs;
    public static GameObject[] EnemyPrefabs;

    [SerializeField] Wave[] waves;
    public Wave[] Waves { get => waves; }
    private Wave currentWave;
    private int currentWaveIndex;
    public int CurrentWave { get => currentWaveIndex; }
    private readonly List<Batch> activeBatches = new();
    private int nextBatch;

    private State spawnState = State.BetweenWaves;
    private float spawnCooldown;
    private float cooldownElapsed;

    [SerializeField] float secondsBetweenWaves = 5.0f;
    private float elapsedSecondsBetweenWaves;

    private int spawnedEnemies;
    private readonly List<Enemy> aliveEnemies = new();

    [SerializeField] Transform parentFolder;
    [SerializeField] Meteor meteor;
    [SerializeField] Text wonText;
    private LevelDataGrid levelData;

    [SerializeField] WaveCounter waveCounter;
        
    [SerializeField] RectTransform waveIndicator;
    [SerializeField] Image waveTimer;
    [SerializeField] Vector2[] indicatorPositions;

    [Header("Shroom Unlocks")]
    [SerializeField] GameObject shroomUnlockTooltip;
    [SerializeField] Image shroomIcon;
    [SerializeField] Sprite[] shroomSprites = new Sprite[4];
    [SerializeField] float unlockTooltipDuration = 2;

    // Tutorial
    private TutorialManager tutorial;

    private void Awake()
    {
        EnemyPrefabs = enemyPrefabs;

        levelData = GetComponent<LevelDataGrid>();

        currentWave = waves[currentWaveIndex];

        spawnCooldown = currentWave.duration / currentWave.GetGroupedEnemyCount();

        tutorial = GetComponent<TutorialManager>();
    }

    private void Update()
    {
        if (InteractionManager.gamePaused) return;

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
        if (elapsedSecondsBetweenWaves >= unlockTooltipDuration)
        {
            shroomUnlockTooltip.SetActive(false);
        }
        else if (currentWaveIndex > 0 && currentWaveIndex < 5)
        {
            shroomUnlockTooltip.SetActive(true);
            shroomIcon.sprite = shroomSprites[currentWaveIndex];

            if (elapsedSecondsBetweenWaves == 0.0f && currentWaveIndex > 0)
            {
                GetComponent<InteractionManager>().UnlockShroom(currentWaveIndex);
            }
        }

        if (elapsedSecondsBetweenWaves >= secondsBetweenWaves)
        {
            waveCounter.IncWaveCounter();

            spawnState = State.Spawning;
            elapsedSecondsBetweenWaves = 0.0f;

            //waveIndicator.gameObject.SetActive(false);
        }
        else
        {
            //waveIndicator.position = indicatorPositions[currentSpawnPointIndex];
            //waveIndicator.gameObject.SetActive(true);

            //waveTimer.fillAmount = 1 - (elapsedSecondsBetweenWaves / secondsBetweenWaves);
            elapsedSecondsBetweenWaves += Time.deltaTime;
        }
    }

    private void SpawningState()
    {
        if (cooldownElapsed < spawnCooldown)
        {
            cooldownElapsed += Time.deltaTime;
            return;
        }
        cooldownElapsed = 0;

        RefillActiveBatches();

        if (activeBatches.Count == 0)
        {
            spawnState = State.WaitingForWaveEnd;
            return;
        }

        int i = 0;
        while (i < activeBatches.Count)
        {
            Enemy enemy = SpawnNext(activeBatches[i]).GetComponent<Enemy>();

            enemy.Initialise(meteor, levelData);

            aliveEnemies.Add(enemy);

            activeBatches[i].nextEnemy++;

            if (activeBatches[i].nextEnemy >= activeBatches[i].amount)
            {
                activeBatches.RemoveAt(i);
                continue;
            }

            spawnedEnemies++;
            i++;
        }


        // Tutorial Activation

        if (currentWaveIndex == tutorial.warningWave && !tutorial.warningHasPlayed)
        {
            tutorial.elapsedWarningWaitTime += Time.deltaTime;

            if (tutorial.elapsedWarningWaitTime >= tutorial.warningWaitTime)
            {
                tutorial.StartTutorial(TutorialManager.Tutorial.Warning);
            }
        }

        if (currentWaveIndex == tutorial.sellingWave && !tutorial.sellingHasPlayed)
        {
            tutorial.StartTutorial(TutorialManager.Tutorial.Selling);
        }
    }

    private void WaitingForWaveEndState()
    {
        if (aliveEnemies.Count == 0)
        {
            if (currentWaveIndex + 1 < waves.Length)
            {
                Debug.Log("Next Wave Starting in " + secondsBetweenWaves + " Seconds");

                InitialiseNextWave();

                GenericUtility.DestroyAllDeadChildren(parentFolder.transform);
            }
            else
            {
                wonText.enabled = true;

                Debug.Log("Final Wave Defeated");

                StartCoroutine(GameWon());
            }

        }
    }

    private GameObject SpawnNext(Batch batch)
    {
        GameObject prefabToSpawn = batch.Enemy;

        Vector3 spawnPos;
        if (batch.useDefaultSpawn) spawnPos = currentWave.spawnPosition;
        else spawnPos = batch.spawnPosition;


        GameObject enemyObject = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, parentFolder);

        enemyObject.name = "Wave " + (currentWaveIndex + 1) + " Enemy";

        return enemyObject;
    }

    private void RefillActiveBatches()
    {
        if (activeBatches.Count == 0)
        {
            while (nextBatch < currentWave.batches.Length)
            {
                if (activeBatches.Count == 0 || currentWave.batches[nextBatch].withPrevious)
                {
                    activeBatches.Add(currentWave.batches[nextBatch]);
                    nextBatch++;
                }
                else break;
            }
        }
    }

    private void InitialiseNextWave()
    {
        spawnState = State.BetweenWaves;

        currentWaveIndex++;
        currentWave = waves[currentWaveIndex];

        spawnedEnemies = 0;
        spawnCooldown = currentWave.duration / currentWave.GetGroupedEnemyCount();

        nextBatch = 0;
    }

    private IEnumerator GameWon()
    {
        yield return new WaitForSeconds(5);
        LoadScene(GetActiveScene().buildIndex);
    }
}