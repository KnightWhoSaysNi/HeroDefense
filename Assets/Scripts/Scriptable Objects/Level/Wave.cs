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
    /// False would spawn enemies as they are listed in the enemies field. True would randomize their order of appearance.
    /// </summary>
    [Space(5)]
    [Tooltip("Not implemented yet. Does nothing.")]
    public bool shouldRandomizeEnemies;
    /// <summary>
    /// How fast enemies will spawn - every specified second.
    /// </summary>
    [Tooltip("How fast enemies will spawn - every specified second.")]
    [Range(0, 100)] public float spawnRate; // ADD TO CONST

    [Space(10)]
    /// <summary>
    /// A list enemies along with additional wave information, like their min/max number and spawn delay.
    /// </summary>
    public List<WaveELement> waveElements;

    // TODO maybe set spawnRate for each type separately                     
    // TODO Create a reward for finishing a wave    
}