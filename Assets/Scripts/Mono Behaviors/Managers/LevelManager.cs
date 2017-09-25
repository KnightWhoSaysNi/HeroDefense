using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour // TODO Create custom editor
{
    private static Level currentLevel;
    private static Transform playerStartBearings;
    private static Transform spawnPoint; 
    private static Transform endPoint;
    private static Transform enemyParent;

    private static int currentEnergy; // TODO Use this
    
    // Wave specific fields
    private Wave currentWave;
    private WaveELement currentWaveElement;
    private WaitForSeconds spawnCooldown;
    private WaitForSeconds additionalSpawnDelay = new WaitForSeconds(0);
    private WaitForSeconds waveStartDelay;
    private WaitForSeconds waveEndDelay;
    private int waveOrdinalNumber;
    private int numberOfAliveEnemies;
    private int numberOfEnemies;
    private bool hasStartDelay;
    private bool hasEndDelay;
    private bool shouldEnemiesAppear;
    private bool hasDelayBeforeSpawning;

    public static event System.Action<Level> LevelStarted;
    public static event System.Action<int> WaveStarting;

    /// <summary>
    /// Transform holding players start position and rotation for the current level.
    /// </summary>
    public static Transform PlayerStartBearings
    {
        get
        {
            return playerStartBearings;
        }
    }

    /// <summary>
    /// Sets up current level for the level manager, along with some other important parameters.
    /// </summary>
    /// <param name="levelManagerHelper">Class holding information of importance.</param>
    public static void SetUpLevel(LevelManagerHelper levelManagerHelper)
    {
        currentLevel = levelManagerHelper.level;
        //currentEnergy = currentLevel.energy; // TODO Add Energy to level

        playerStartBearings = levelManagerHelper.playerStartBearings;
        spawnPoint = levelManagerHelper.spawnPoint;
        endPoint = levelManagerHelper.endPoint;
        enemyParent = levelManagerHelper.enemyParent; 
    }

    private void Awake()
    {
        // Instead of making this class a "singleton" a simple check is made. This way only GameManager game object can have this mono behavior attached
        if (GetComponent<GameManager>() == null)
        {
            throw new UnityException($"{gameObject.name} game object has a {typeof(LevelManager)} MonoBehavior attached. Only GameManager is allowed to have that script.");
        }

        Enemy.EnemyDied += OnEnemyDied;
    }

    private void Update()
    {
        // TODO Only after clicking Start level should the spawning start and that should be registered in ui manager?
        // TEST
        if (Input.GetKeyDown(KeyCode.G))
        {
            numberOfAliveEnemies = 0;
            StopAllCoroutines();
            StartCoroutine(PlayLevel());
        }
    }

    private IEnumerator PlayLevel()
    {
        // Let anyone interested know that level has started
        LevelStarted?.Invoke(currentLevel);

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
                // Let anyone interested know which wave is starting
                waveOrdinalNumber = (i + 1) * (j + 1);
                WaveStarting?.Invoke(waveOrdinalNumber);

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
                hasDelayBeforeSpawning = currentWaveElement.delayBeforeSpawning > 0;
                if (hasDelayBeforeSpawning)
                {
                    additionalSpawnDelay = new WaitForSeconds(currentWaveElement.delayBeforeSpawning);                
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
        // TODO Create an enemy object pool 
        GameObject enemyObject = Instantiate(enemy.gameObject, spawnPoint.position, spawnPoint.rotation, enemyParent);
        Enemy instantiatedEnemy = enemyObject.GetComponent<Enemy>();
        instantiatedEnemy.moveAgent.SetTarget(endPoint.position);
        
        numberOfAliveEnemies++;
    }

    /// <summary>
    /// Reduces the number of alive enemies.
    /// </summary>    
    private void OnEnemyDied(Enemy enemy, Collider enemyCollider)
    {
        numberOfAliveEnemies--;
    }

    /// <summary>
    /// Checks if there are still alive enemies in the level. If all are all dead or have finished their route the wave is finished.
    /// </summary>
    private bool IsWaveFinished()
    {
        return numberOfAliveEnemies == 0; 
    }
}
