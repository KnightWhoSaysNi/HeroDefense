using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameplayUIData : UIData
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

    [Header("Enemy Health")]
    public GameObject enemyHealthCanvas;
    [Space(3)]
    public Text enemyHealth;
    public Text enemyArmor;
    public Slider enemyHealthBar;

    [Header("Gameplay Menu")]
    public GameObject menuCanvas;
    [Space(3)]
    public GameObject levelStart;
    public GameObject levelEnd;
    public GameObject pauseMenu;
    [Space(5)]
    public Button levelStartTestButton;
    [Space(3)]
    public Button continueButton;
    public Button optionsButton;
    public Button goToMainMenuButton;

    [Header("Message Display")]
    public GameObject messagingCanvas;
    [Space(3)]
    public Text message;

    [Header("Crosshair")]
    public GameObject crosshair;    
}
