using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour // TODO Create custom editor
{
    public List<SceneLevelPair> allLevels;
    private LevelData levelData;

    private string newlyLoadedSceneName;
    private bool isResetRequired;

    // Level specific fields
    private Level currentLevel;
    private int currentEnergy;
    private bool canStartLevel;
    private bool isLevelOngoing;
    private bool haveAllWavesSpawned;

    // Wave specific fields
    private Wave currentWave;
    private WaveELement currentWaveElement;
    private WaitForSeconds spawnCooldown;
    private WaitForSeconds additionalSpawnDelay;
    private WaitForSeconds waveStartDelay;
    private WaitForSeconds waveEndDelay;
    private int waveOrdinalNumber;
    private int numberOfAliveEnemies;
    private int numberOfEnemies;
    private bool hasStartDelay;
    private bool hasEndDelay;
    private bool shouldEnemiesAppear;
    private bool hasDelayBeforeSpawning;

    public static event System.Action LevelRestarted;
    public static event System.Action PlayerWon;
    public static event System.Action PlayerLost;

    public int CurrentEnergy
    {
        get
        {
            return currentEnergy;
        }
        set
        {
            currentEnergy = value;

            if (currentEnergy <= 0)
            {
                // Game over
                currentEnergy = 0;
                isLevelOngoing = false;
                StopAllCoroutines();

                PlayerLost?.Invoke();
            }

            UIManager.Instance.UpdateEnergyInfo(currentEnergy, currentLevel.startEnergy);
        }
    }    

    #region - "Singleton" Instance -
    private static LevelManager instance;

    public static LevelManager Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling LevelManager.Instance before it is set! Change script execution order.");
            }

            return instance;
        }
    }

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion    

    /// <summary>
    /// Sets up level data for the LevelManager.
    /// </summary>
    /// <remarks>This is called every time a new level scene loads.</remarks>
    public void SetUpLevel(LevelData levelData)
    {
        this.levelData = levelData;
        SetUpPlayerBearings();
        PlacementManager.Instance.placeableParent = levelData.placeableParent;
    }

    /// <summary>
    /// Transform holding players start position and rotation for the current level.
    /// </summary>
    public Transform PlayerStartBearings
    {
        get
        {
            return levelData.playerStartBearings;
        }
    }

    public void RestartLevel()
    {
        isLevelOngoing = false;
        StopAllCoroutines();

        PlaceablePool.Instance.ReclaimAllObjects();
        EnemyPool.Instance.ReclaimAllObjects();
        ResetLevel();

        LevelRestarted?.Invoke();
    }
   
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneLoader.Instance.MainMenuLoading += OnMainMenuLoading;
        UIManager.LevelStarted += OnLevelStarted;
        Enemy.EnemyDied += OnEnemyDied;
    }

    private void Update()
    {
        // TEST
        if (canStartLevel && Input.GetKeyDown(KeyCode.G))
        {
            canStartLevel = false;            
            StopAllCoroutines();
            StartCoroutine(PlayLevel());
        }

        if (isResetRequired && UIManager.Instance.IsReadyToReceiveUpdates)
        {
            isResetRequired = false;
            ResetLevel();
        }

        if (isLevelOngoing && haveAllWavesSpawned && IsWaveFinished())
        {
            isLevelOngoing = false;
            PlayerWon?.Invoke();
        }
    }

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode)
    {
        if (loadedScene.name != Constants.MainMenuSceneName && loadedScene.name != Constants.GameplayUISceneName) 
        {
            newlyLoadedSceneName = loadedScene.name;
            isResetRequired = true;

            UpdateCurrentLevel(newlyLoadedSceneName);
        }
    }

    private void ResetLevel()
    {
        numberOfAliveEnemies = 0;
        waveOrdinalNumber = 0;
        isLevelOngoing = false;
        haveAllWavesSpawned = false;

        currentEnergy = currentLevel.startEnergy;
        Player.Instance.Gold = currentLevel.startGold;
                
        UIManager.Instance.UpdateWaveInfo(0, currentLevel.TotalWaveCount);
        UIManager.Instance.UpdateEnergyInfo(currentEnergy, currentLevel.startEnergy);

        SetUpPlayerBearings();
    }

    /// <summary>
    /// Sets the current level to the one paired with the provided scene name.
    /// </summary>
    private void UpdateCurrentLevel(string sceneName)
    {
        currentLevel = null;

        for (int i = 0; i < allLevels.Count; i++)
        {
            if (sceneName == allLevels[i].sceneName)
            {
                currentLevel = allLevels[i].level;
                break;
            }
        }

        if (currentLevel == null)
        {
            // There is no level associated with the loaded scene
            throw new UnityException($"{sceneName} isn't added to the collection of all levels in the {typeof(LevelManager)}");
        }
    }

    private void OnMainMenuLoading()
    {
        if (isLevelOngoing)
        {
            // Scene is changing but level is still ongoing
            isLevelOngoing = false;
            StopAllCoroutines();
        }
    }

    private void OnLevelStarted()
    {
        canStartLevel = true;
    }

    /// <summary>
    /// Sets up player position and rotation for the newly loaded level.
    /// </summary>
    private void SetUpPlayerBearings()
    {
        Player.Instance.transform.position = levelData.playerStartBearings.position;        
        Player.Instance.transform.rotation = levelData.playerStartBearings.rotation;
        GameManager.Instance.playerCamera.transform.localEulerAngles = Vector3.zero;
    }

    private IEnumerator PlayLevel()
    {
        isLevelOngoing = true;

        int numberOfLevelElements = currentLevel.levelElements.Count;

        // Go through each level element 
        for (int i = 0; i < numberOfLevelElements; i++)
        {
            currentWave = currentLevel.levelElements[i].wave;

            // Check if wave has start delay
            if (currentWave.startDelay > 0)
            {
                hasStartDelay = true;
                waveStartDelay = new WaitForSeconds(currentWave.startDelay);
            }

            // Check if wave has end delay
            if (currentWave.endDelay > 0)
            {
                hasEndDelay = true;
                waveEndDelay = new WaitForSeconds(currentWave.endDelay);
            }

            // Repeat playing the wave waveCount times
            for (int j = 0; j < currentLevel.levelElements[i].waveCount; j++)
            {
                // Let UIManager update the wave information
                waveOrdinalNumber++;
                bool isFinalWave = waveOrdinalNumber == currentLevel.TotalWaveCount;
                UIManager.Instance.UpdateWaveInfo(waveOrdinalNumber, currentLevel.TotalWaveCount);
                UIManager.Instance.ShowCountdown(currentWave.startDelay, isFinalWave);

                // Wave start delay
                if (hasStartDelay)
                {
                    yield return waveStartDelay;
                }

                // Play current wave
                yield return PlayWave();

                while (!IsWaveFinished())
                {
                    yield return null;
                }

                // Wave end delay
                if (hasEndDelay)
                {
                    yield return waveEndDelay;
                }
            }
        }

        // TODO At this point all waves have spawned. When all enemies are dead the level is no longer ongoing and the player has won
        haveAllWavesSpawned = true;
    }

    private IEnumerator PlayWave()
    {
        int numberOfWaveElements = currentWave.waveElements.Count;
        spawnCooldown = new WaitForSeconds(currentWave.spawnRate);

        // Go through each wave element
        for (int i = 0; i < numberOfWaveElements; i++)
        {

            currentWaveElement = currentWave.waveElements[i];
            shouldEnemiesAppear = currentWaveElement.chanceOfAppearing >= Random.Range(0f, 1f); 

            if (shouldEnemiesAppear)
            {
                hasDelayBeforeSpawning = currentWaveElement.additionalSpawnDelay > 0;
                if (hasDelayBeforeSpawning)
                {
                    additionalSpawnDelay = new WaitForSeconds(currentWaveElement.additionalSpawnDelay);                
                }                

                // Get the exact number of enemies for this wave element
                numberOfEnemies = Random.Range(currentWaveElement.minNumberOfEnemies, currentWaveElement.maxNumberOfEnemies + 1);

                for (int j = 0; j < numberOfEnemies; j++)
                {
                    SpawnEnemy(currentWaveElement.enemy);

                    if (hasDelayBeforeSpawning)
                    {
                        yield return additionalSpawnDelay;
                    }
                    yield return spawnCooldown;
                }
            }
        }

        // All enemies in the current wave have spawned
    }

    /// <summary>
    /// Instantiates an enemy at a spawn point and sets its agent's destination.
    /// </summary>
    private void SpawnEnemy(Enemy enemy)
    {
        Enemy spawnedEnemy = EnemyPool.Instance.GetObject(enemy.enemyType, levelData.enemyParent, null, levelData.endPoint.position);     
        numberOfAliveEnemies++;
    }

    /// <summary>
    /// Reduces the number of alive enemies and reports to the player how much gold and experience he got if he killed the enemy.
    /// Doesn't reward the player if the enemy has finished the level but reduces the current energy.
    /// </summary>    
    private void OnEnemyDied(Enemy enemy, Collider enemyCollider, bool hasFinishedLevel)
    {
        if (isLevelOngoing)
        {
            numberOfAliveEnemies--;

            if (hasFinishedLevel)
            {
                CurrentEnergy -= enemy.EnergyDrain;
            }
            else
            {
                // Enemy was killed by the player
                Player.Instance.Gold += enemy.GoldReward;
                int experienceReward = (enemy.Level / Player.Instance.Level) * enemy.BaseExperienceReward;
                Player.Instance.Experience += experienceReward;
            }
        }
    }

    /// <summary>
    /// Checks if there are still alive enemies in the level. If all are all dead or have finished their route the wave is finished.
    /// </summary>
    private bool IsWaveFinished()
    {
        return numberOfAliveEnemies == 0; 
    }
}

[System.Serializable]
public struct SceneLevelPair
{
    public string sceneName;
    public Level level;

    public SceneLevelPair(string sceneName, Level level)
    {
        this.sceneName = sceneName;
        this.level = level;
    }
}