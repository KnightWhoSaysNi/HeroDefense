using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds wave information for an enemy type.
/// </summary>
[System.Serializable]
public class WaveEnemyData
{
    /// <summary>
    /// Enemy that could appear in this wave.
    /// </summary>
    public EnemyData enemyData;
    /// <summary>
    /// Minimum number of enemies that could appear in this wave.
    /// </summary>
    [Range(1, 1000)] public int minNumberOfEnemies; // ADD TO CONST
    /// <summary>
    /// Maximum number of enemies that could appear in this wave.
    /// </summary>
    [Range(1, 1000)] public int maxNumberOfEnemies; // ADD TO CONST
    /// <summary>
    /// Chance of enemies (even) appearing in this wave, regardless of their number. Default is 1 (100%).
    /// </summary>
    [Range(0, 1)] public float chanceOfAppearing = 1; // ADD TO CONST
    public float delayBeforeSpawning = 0;
}
