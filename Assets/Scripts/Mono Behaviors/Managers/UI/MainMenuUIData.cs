using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MainMenuUIData : InspectorData
{
    [Header("Main Menu")]
    public GameObject mainMenu;
    public Button encyclopedia;
    public Button options;
    public Button quit;
    [Header("Levels")]
    // TODO Change this when new levels are created
    public GameObject playMenu;
    public Button tutorialLevel;
    public Button level01;
    public Button level02;
}
