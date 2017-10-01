using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameplayUIData : InspectorData
{
    [Header("Player Stats")]
    public Text playerLevel;
    public Text playerExperience;
    public Slider playerExperienceBar;
    [Space(5)]
    public Text playerGold;

    [Header("Level Information")]
    public Text wave;
    public Text energy;

    [Header("Slots")]
    public GameObject slotsCanvas ;
    [Space(3)]
    public Slot[] slots;

    [Header("Enemy Information")]
    public GameObject enemyCanvas;
    [Space(3)]
    public Text enemyHealth;
    public Text enemyArmor;
    public Slider enemyHealthBar;

    [Header("Placeable Information")]
    public GameObject placeableCanvas;
    [Space(3)]
    public Text placeableSellPrice;

    [Header("Gameplay Menu")]
    public GameObject menuCanvas;
    public GraphicRaycaster menuGraphicRaycaster;

    [Header("Level Start")]
    public GameObject levelStartPanel;
    [Space(3)]
    public Button levelStartTest;
    
    [Header("Level End")]
    public GameObject levelEndPanel;
    [Space(3)]
    public GameObject loseText;
    public GameObject winText;
    public Button levelEndGoToMainMenu;
    public Button levelEndRestart;
    public Button levelEndGoToNextLevel;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    [Space(3)]
    public Button pauseContinue;
    public Button pauseRestart;
    public Button pauseOptions;
    public Button pauseGoToMainMenu;
    public Button pauseQuit;

    [Header("Message Display")]
    public GameObject messagingCanvas;
    [Space(3)]
    public Text message;

    [Header("Crosshair")]
    public GameObject crosshair;    
}
