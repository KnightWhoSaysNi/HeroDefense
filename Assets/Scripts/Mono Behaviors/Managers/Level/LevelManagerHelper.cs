using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManagerHelper : MonoBehaviour 
{
    public LevelData levelData;

    private void Awake()
    {
        if (!levelData.AreAllElementsSet())
        {
            throw new UnityException($"{typeof(LevelManagerHelper)} class doesn't have all of its fields set up.");
        }

        LevelManager.Instance.SetUpLevel(levelData);
    }
}
