using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIHelper : MonoBehaviour
{
    public MainMenuUIData mainMenuUIData;

    private void Awake()
    {
        if (!mainMenuUIData.AreAllElementsSet())
        {
            throw new UnityException($"{typeof(MainMenuUIHelper)} class doesn't have all of its fields set up.");
        }

        UIManager.Instance.SetUpMainMenuUI(mainMenuUIData);
    }
}
