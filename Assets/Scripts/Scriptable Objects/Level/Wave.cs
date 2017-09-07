using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Wave")]
public class Wave : ScriptableObject
{
    /// <summary>
    /// Delay before the wave starts.
    /// </summary>
    [Range(0, 100)] public float startDelay; // ADD TO CONST
    /// <summary>
    /// Delay after the wave is finished.
    /// </summary>
    [Range(0, 100)] public float endDelay; // ADD TO CONST

    /// <summary>
    /// A list of all enemy types and information about their number, chance of appearing and additional delay (e.g. for mini bosses).
    /// </summary>
    public List<WaveEnemyData> enemies;
    /// <summary>
    /// False would spawn enemies as they are listed in the enemies field. True would randomize their order of appearance.
    /// </summary>
    [Tooltip("Not implemented yet. Does nothing")] // TODO
    public bool shouldRandomizeEnemies;
    /// <summary>
    /// Every specified second an individual enemy is spawned.
    /// </summary>
    [Tooltip("Every specified second an individual enemy is spawned")]
    [Range(0,100)] public float spawnRate; // ADD TO CONST

    // TODO maybe set spawnRate for each type separately                     
    // TODO Create a reward for finishing a wave    
}