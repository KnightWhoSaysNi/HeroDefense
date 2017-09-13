using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Level level;
    public Transform spawnPoint;
    public Transform endPoint;
    public Transform enemyParent;
    
    // Wave specific fields
    private Wave currentWave;
    private int currentWaveIndex;
    private WaitForSeconds spawnCooldown;
    private WaitForSeconds additionalSpawnDelay = new WaitForSeconds(0);
    private int numberOfAliveEnemies;
    private int numberOfEnemies;
    private bool shouldEnemiesAppear;
    private bool areAllEnemiesSpawned;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopAllCoroutines();
            StartCoroutine(PlayLevel());
        }
    }

    private IEnumerator PlayLevel()
    {
        int numberOfLevelElements = level.levelElements.Count;

        // Go through each level element 
        for (int i = 0; i < numberOfLevelElements; i++)
        {
            // Repeat playing the wave waveCount times
            for (int j = 0; j < level.levelElements[i].waveCount; j++)
            {
                // Wave start delay
                yield return new WaitForSeconds(level.levelElements[i].wave.startDelay);

                currentWave = level.levelElements[i].wave;
                currentWaveIndex = i;

                // Play current wave
                yield return PlayWave();

                while (!IsWaveFinished())
                {
                    yield return null;
                }

                // Wave end delay
                yield return new WaitForSeconds(level.levelElements[i].wave.endDelay);
            }
        }
    }

    // TODO Use System.Random and set a global random seed for saving/loading?
    private IEnumerator PlayWave()
    {
        int numberOfWaveElements = currentWave.waveElements.Count;
        spawnCooldown = new WaitForSeconds(currentWave.spawnRate);

        // Go through each wave element
        for (int i = 0; i < numberOfWaveElements; i++)
        {
            shouldEnemiesAppear = currentWave.waveElements[i].chanceOfAppearing >= Random.Range(0f, 1f); // ADD TO CONST values in range 0 and 1

            if (shouldEnemiesAppear)
            {
                additionalSpawnDelay = new WaitForSeconds(currentWave.waveElements[i].delayBeforeSpawning);                

                // Get the exact number of enemies for this wave element
                numberOfEnemies = Random.Range(currentWave.waveElements[i].minNumberOfEnemies, currentWave.waveElements[i].maxNumberOfEnemies + 1);

                for (int j = 0; j < numberOfEnemies; j++)
                {
                    SpawnEnemy(currentWave.waveElements[i].enemy);

                    yield return additionalSpawnDelay;
                    yield return spawnCooldown;
                }
            }
        }

        // All enemies in the current wave have spawned
        areAllEnemiesSpawned = true;
    }

    /// <summary>
    /// Instantiates an enemy at a spawn point and sets its agent's destination.
    /// </summary>
    private void SpawnEnemy(Enemy enemy)
    {
        // TODO Create an enemy object pool 
        GameObject enemyObj = Instantiate(enemy.gameObject, spawnPoint.position, spawnPoint.rotation, enemyParent);
        Enemy instantiatedEnemy = enemyObj.GetComponent<Enemy>();
        instantiatedEnemy.moveAgent.SetTarget(endPoint.position);
        
        numberOfAliveEnemies++;
    }

    private bool IsWaveFinished()
    {
        // TODO Decrease the number of alive enemies when they die. Create an event in the enemy class perhaps
        return numberOfAliveEnemies == 0; 
    }

}
