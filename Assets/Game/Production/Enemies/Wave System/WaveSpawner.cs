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
    [SerializeField] Meteor meteor;
    [SerializeField] Text wonText;
    private LevelDataGrid levelData;
        
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
        levelData = GetComponent<LevelDataGrid>();

        CalculateSpawns();

        currentWave = waves[currentWaveIndex];

        spawnCooldown = currentWave.durationInSeconds / currentWave.enemyCount;

        GetComponent<InteractionManager>().UnlockShroom(0);

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
            spawnState = State.Spawning;
            elapsedSecondsBetweenWaves = 0.0f;

            waveIndicator.gameObject.SetActive(false);
        }
        else
        {
            waveIndicator.position = indicatorPositions[currentSpawnPointIndex];
            waveIndicator.gameObject.SetActive(true);

            waveTimer.fillAmount = 1 - (elapsedSecondsBetweenWaves / secondsBetweenWaves);
            elapsedSecondsBetweenWaves += Time.deltaTime;
        }
    }

    private void SpawningState()
    {
        if (cooldownElapsed >= spawnCooldown)
        {
            Enemy enemy = SpawnEnemy().GetComponent<Enemy>();
            enemy.meteor = meteor;
            enemy.meteorTransform = meteor.transform;
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
            //UpdateWaveCounterUI(); // ** commented this out bcs we not using it anymore :) - James
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

    private IEnumerator GameWon()
    {
        yield return new WaitForSeconds(5);
        LoadScene(GetActiveScene().buildIndex);
    }

    private void InitialiseNextWave()
    {
        spawnState = State.BetweenWaves;

        currentWaveIndex++;
        enemyNumber = 0;
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

        enemyObject.name = "Child " + (currentWaveIndex + 1) + "-" + enemyNumber;
        enemyNumber++;

        return enemyObject;
    }

    /* ** commented this out bcs we not using it anymore :) - James
     * 
    /// <summary>
    /// This is Lochlan's code for updating the WaveCounter UI element
    /// </summary>
    private void UpdateWaveCounterUI()
    {
        float waveProgress = (float)(currentWaveIndex +1)/ (float)waves.Length;
        waveCounterUI.AnimateBitsFalling();
        waveCounterUI.SetWaveCounterFill(waveProgress);
    }
    */
}