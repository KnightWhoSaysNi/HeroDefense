using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Level level;
    public Transform spawnPoint;
    public Transform endPoint;
    public Transform enemiesParent;
    
    // Wave specific fields
    private Wave currentWave;
    private int currentWaveIndex;
    private WaitForSeconds spawnCooldown;
    private WaitForSeconds additionalSpawnDelay = new WaitForSeconds(0);
    private int numberOfAliveEnemies;
    private int numberOfEnemies;
    private bool shouldEnemiesAppear;
    private bool areAllEnemiesSpawned;
        

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
        int numberOfWaves = level.levelElements.Count;

        // Go through each wave in the level
        for (int i = 0; i < numberOfWaves; i++)
        {
            // Wave start delay
            yield return new WaitForSeconds(level.levelElements[i].wave.startDelay);

            currentWave = level.levelElements[i].wave;
            currentWaveIndex = i;

            // Go through current wave 
            yield return PlayWave();

            while (!IsWaveFinished())
            {
                yield return null;
            }

            // Wave end delay
            yield return new WaitForSeconds(level.levelElements[i].wave.endDelay);
        }
    }

    // TODO Use System.Random and set a global random seed for saving/loading?
    private IEnumerator PlayWave()
    {
        int numberOfDifferentEnemies = currentWave.waveElements.Count;
        spawnCooldown = new WaitForSeconds(currentWave.spawnRate);

        // Go through each enemy type
        for (int i = 0; i < numberOfDifferentEnemies; i++)
        {
            shouldEnemiesAppear = currentWave.waveElements[i].chanceOfAppearing >= Random.Range(0f, 1f); // ADD TO CONST values in range 0 and 1

            if (shouldEnemiesAppear)
            {
                additionalSpawnDelay = new WaitForSeconds(currentWave.waveElements[i].delayBeforeSpawning);                

                // Get the exact number of enemies for this enemy type
                numberOfEnemies = Random.Range(currentWave.waveElements[i].minNumberOfEnemies, currentWave.waveElements[i].maxNumberOfEnemies);

                for (int j = 0; j < numberOfEnemies; j++)
                {
                    SpawnEnemy(currentWave.waveElements[i].enemy);

                    yield return additionalSpawnDelay;
                    yield return spawnCooldown;
                }
            }
        }

        // All enemies in the current wave spawned
        areAllEnemiesSpawned = true;
    }

    /// <summary>
    /// Spawns individual enemy with the given enemy data.
    /// </summary>
    /// <param name="enemy">Information on the enemy to be spawned.</param>
    private void SpawnEnemy(Enemy enemy)
    {
        // TODO Create an enemy object pool 

        //GameObject enemyObj = Instantiate(enemy.enemyPrefab, spawnPoint.position, spawnPoint.rotation, enemiesParent);
        //// TODO just a test
        //enemy.GetComponent<MoveAgent>().waypoints = new Transform[] { spawnPoint, endPoint };
        //numberOfAliveEnemies++;
    }

    private bool IsWaveFinished()
    {
        // TODO Check if both checks are necessairy, it could be that only the number of alive enemies is needed
        // TODO Decrease the number of alive enemies when they die. Create an event in the enemy class perhaps
        return areAllEnemiesSpawned && (numberOfAliveEnemies == 0); 
    }

}
