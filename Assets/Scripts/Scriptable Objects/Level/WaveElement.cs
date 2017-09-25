using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds wave information for a specific enemy.
/// </summary>
[System.Serializable]
public class WaveELement
{
    /// <summary>
    /// Enemy that could appear in a wave.
    /// </summary>
    public Enemy enemy;

    /// <summary>
    /// Minimum number of enemies that could appear in a wave.
    /// </summary>
    [Range(1, 1000)] public int minNumberOfEnemies; // ADD TO CONST
    /// <summary>
    /// Maximum number of enemies that could appear in a wave.
    /// </summary>
    [Range(1, 1000)] public int maxNumberOfEnemies; // ADD TO CONST
    /// <summary>
    /// Chance of enemies (even) appearing in a wave, regardless of their number. Default is 1 (100%).
    /// </summary>
    [Range(0, 1)] public float chanceOfAppearing = 1; // ADD TO CONST
    public float delayBeforeSpawning = 0;
}
