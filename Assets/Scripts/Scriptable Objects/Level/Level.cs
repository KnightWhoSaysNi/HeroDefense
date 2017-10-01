using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    /// <summary>
    /// A list of waves along with additional level information, like the wave count.
    /// </summary>
    public List<LevelElement> levelElements;

    public int startEnergy; 
    public int startGold;

    private int totalWaveCount;
    public int TotalWaveCount
    {
        get
        {
            if (totalWaveCount == 0)
            {
                // First time calling this property
                for (int i = 0; i < levelElements.Count; i++)
                {
                    totalWaveCount += levelElements[i].waveCount;
                }
            }

            return totalWaveCount;
        }
    }
}
