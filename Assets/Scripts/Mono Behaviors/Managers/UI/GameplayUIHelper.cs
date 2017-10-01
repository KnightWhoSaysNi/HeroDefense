using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIHelper : MonoBehaviour
{
    public GameplayUIData gameplayUIData;

    private void Awake()
    {        
        if (!gameplayUIData.AreAllElementsSet())
        {
            throw new UnityException($"{typeof(GameplayUIHelper)} class doesn't have all of its fields set up.");
        }

        UIManager.Instance.SetUpGameplayUI(gameplayUIData);
    }
}
