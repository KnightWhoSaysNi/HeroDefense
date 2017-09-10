using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds level information for a specific wave.
/// </summary>
[System.Serializable]
public class LevelElement
{
    public Wave wave;
    /// <summary>
    /// How many times the wave will play in a level. Default should be 1 as anything less makes no sense.
    /// </summary>
    public int waveCount = 1;
}
