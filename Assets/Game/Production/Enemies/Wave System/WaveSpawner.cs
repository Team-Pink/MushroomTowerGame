using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.SceneManagement.SceneManager;

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

    [SerializeField] Spawnpoint spawnOverride = null;
    public Spawnpoint spawn
    {
        get
        {
            if (spawnOverride == null)
            {
                Debug.LogError("No spawn point set");
            }
            return spawnOverride;
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

    [SerializeField] Spawnpoint defaultSpawn = null;
    public Spawnpoint spawn
    {
        get
        {
            if (defaultSpawn == null)
            {
                Debug.LogError("No wave default spawn point set");
            }
            return defaultSpawn;
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
    [SerializeField] GameObject winScreen;
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

    [Header("First Wave = 1 // If wave doc says 1, write 2")]
    [SerializeField] int inkUnlockWave;
    [SerializeField] int slamUnlockWave;
    [SerializeField] int mortarUnlockWave;
    [SerializeField] int laserUnlockWave;

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

        if (elapsedSecondsBetweenWaves >= unlockTooltipDuration)
        {
            shroomUnlockTooltip.SetActive(false);

            if (spawnState != State.BetweenWaves) elapsedSecondsBetweenWaves = 0.0f;
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
        if (elapsedSecondsBetweenWaves < unlockTooltipDuration && currentWaveIndex > 0)
        {
            //shroomUnlockTooltip.SetActive(true); // moved this down so that it works with the new unlockIndex var
            //shroomIcon.sprite = shroomSprites[currentWaveIndex];

            if (elapsedSecondsBetweenWaves == 0.0f && currentWaveIndex > 0)
            {
                int unlockIndex = -1;

                // what shroom need to be unlocked now?
                // index: 1=ink, 2=slam, 3=mortar, 4=laser

                
                if (currentWaveIndex == inkUnlockWave) unlockIndex = 1;

                else if (currentWaveIndex == slamUnlockWave) unlockIndex = 2;

                else if (currentWaveIndex == mortarUnlockWave) unlockIndex = 3;

                else if (currentWaveIndex == laserUnlockWave) unlockIndex = 4;

                GetComponent<InteractionManager>().UnlockShroom(unlockIndex);

                if (unlockIndex != -1)
                {
                    shroomUnlockTooltip.SetActive(true);
                    shroomIcon.sprite = shroomSprites[unlockIndex];
                }
            }
        }

        if (elapsedSecondsBetweenWaves >= secondsBetweenWaves)
        {
            waveCounter.IncWaveCounter();

            spawnState = State.Spawning;

            if (shroomUnlockTooltip.activeSelf == false)
            {
                elapsedSecondsBetweenWaves = 0.0f;
            }

            if (currentWave.spawn != null)
            {
                currentWave.spawn.waveIndicator.SetActive(false);
            }

            foreach (Batch batch in currentWave.batches)
            {
                if (batch.useDefaultSpawn == false)
                {
                    batch.spawn.waveIndicator.SetActive(false);
                }
            }
        }
        else
        {
            if (currentWave.spawn != null)
            {
                currentWave.spawn.waveIndicator.SetActive(true);
                currentWave.spawn.timer.fillAmount = 1 - (elapsedSecondsBetweenWaves / secondsBetweenWaves);
            }

            foreach (Batch batch in currentWave.batches)
            {
                if (batch.useDefaultSpawn == false)
                {
                    batch.spawn.waveIndicator.SetActive(true);
                    batch.spawn.timer.fillAmount = 1 - (elapsedSecondsBetweenWaves / secondsBetweenWaves);
                }
            }
            elapsedSecondsBetweenWaves += Time.deltaTime;
        }
    }

    private void SpawningState()
    {
        if (shroomUnlockTooltip.activeSelf == true) elapsedSecondsBetweenWaves += Time.deltaTime;

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
                winScreen.SetActive(true);

                Debug.Log("Final Wave Defeated");

                StartCoroutine(GameWon());
            }

        }
    }

    private GameObject SpawnNext(Batch batch)
    {
        GameObject prefabToSpawn = batch.Enemy;

        Vector3 spawnPos;
        if (batch.useDefaultSpawn) spawnPos = currentWave.spawn.position;
        else spawnPos = batch.spawn.position;


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