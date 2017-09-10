using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorHelper
{
    /// <summary>
    /// Checks if the specified level can be used in play mode.
    /// </summary>
    /// <param name="level">A level whose parameters need to be checked.</param>
    /// <returns>True if it can be used in play mode, false otherwise.</returns>
    public static bool IsLevelValid(Level level)
    {
        if (level == null)
        {
            // The level itself cannot be null
            return false;
        }

        if (level.levelElements == null || level.levelElements.Count == 0)
        {
            // Level must have an instantiated list of level elements 
            // Level must also have at least 1 element in the list // TODO Change this check if waves can be loaded at runtime
            return false;
        }

        for (int i = 0; i < level.levelElements.Count; i++)
        {
            if (level.levelElements[i].waveCount < 1)
            {
                // A wave count that is less than 1 makes no sense, the wave needs to play at least once
                return false;
            }

            bool isWaveValid = IsWaveValid(level.levelElements[i].wave);
            if (!isWaveValid)
            {
                return false;
            }
        }

        // Level is valid and can be used in play mode
        return true;
    }

    /// <summary>
    /// Checks if the specified wave can be used in play mode.
    /// </summary>
    /// <param name="wave">A wave whose parameters need to be checked.</param>
    /// <returns>True if it can be used in play mode, false otherwise.</returns>
    public static bool IsWaveValid(Wave wave)
    {
        if (wave == null)
        {
            // The wave itself cannot be null
            return false;
        }

        if (wave.waveElements == null || wave.waveElements.Count == 0)
        {
            // Wave must have an instantiated list wave elements
            // Wave must also have at least 1 element in the list
            return false;
        }

        for (int i = 0; i < wave.waveElements.Count; i++)
        {
            WaveELement waveElement = wave.waveElements[i];

            if (waveElement == null)
            {
                // Wave element cannot be null
                return false;
            }

            if (waveElement.enemy == null)
            {
                // Wave element's enemy cannot be null
                return false;
            }
        }

        // Wave is valid and can be used in play mode
        return true;
    }

    /// <summary>
    /// Creates a blank wave asset in the Assets/Data/Waves folder or if it doesn't exist in the root Assets.
    /// </summary>
    [MenuItem("Create/Wave")]
    public static void CreateWaveAsset()
    {
        Wave wave = ScriptableObject.CreateInstance<Wave>();

        string waveSaveFolder = "Assets/Data/Waves"; // ADD TO CONST

        // If the wave save folder doesn't exist uses Assets main folder. In both cases unique files names are generated
        string assetPath = AssetDatabase.IsValidFolder(waveSaveFolder) ? 
            AssetDatabase.GenerateUniqueAssetPath(waveSaveFolder + "/Wave.asset") : AssetDatabase.GenerateUniqueAssetPath("Assets/Wave.asset");

        AssetDatabase.CreateAsset(wave, assetPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = wave;              
    }
}
