using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Raycaster
{
    public Sprite emptySlotSprite;
    private GameplayUIData gameplayUIData;
    private MainMenuUIData mainMenuUIData;

    private readonly string levelStartMessage = "Press G to start"; // ADD TO CONST ?
    private bool shouldTogglePauseMenu;
    
    private int activeSlotIndex;
    private Enemy activeEnemy;
    private Player player;

    #region - "Singleton" Instance -
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling UIManager.Instance before it is set! Change script execution order.");
            }

            return instance;
        }
    }

    private void InitializeSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }

    #endregion    


    /// <summary>
    /// Sets up main menu UI controls for the UIManager.
    /// </summary>
    /// <remarks>This is called every time MainMenu scene loads.</remarks>
    public void SetUpMainMenuUI(MainMenuUIData mainMenuUIData)
    {
        this.mainMenuUIData = mainMenuUIData;
        ResetMainMenuUI();
        SetUpMainMenuHandlers();
    }

    /// <summary>
    /// Sets up gameplay UI controls for the UIManager.
    /// </summary>
    /// <remarks>This is called every time GameplayUI scene loads.</remarks>
    public void SetUpGameplayUI(GameplayUIData gameplayUIData)
    {
        this.gameplayUIData = gameplayUIData;
        ResetGameplayUI();
        SetUpGameplayMenuHandlers();
    }

    #region - Main Menu UI -

    public void ToggleCrosshair(bool isActive)
    {
        gameplayUIData.crosshair.SetActive(isActive);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ResetMainMenuUI()
    {
        mainMenuUIData.mainMenu.SetActive(true);
        mainMenuUIData.playMenu.SetActive(false);
    }

    private void SetUpMainMenuHandlers()
    {
        // TODO Add handlers when there's something to add
        //mainMenuUIData.encyclopedia.onClick.AddListener
        //mainMenuUIData.options.onClick.AddListener
        mainMenuUIData.quit.onClick.AddListener(Quit);
        mainMenuUIData.tutorialLevel.onClick.AddListener(() => SceneLoader.Instance.LoadScene("TutorialLevel")); // ADD TO CONST
    }

    #endregion

    #region - Gameplay UI -
    #region - Player Stats -
    public void UpdatePlayerStats()
    {
        UpdatePlayerLevel();
        UpdatePlayerExperience();
        UpdatePlayerGold();
    }

    public void UpdatePlayerLevel()
    {
        gameplayUIData.playerLevel.text = player.Level.ToString();
    }

    public void UpdatePlayerExperience()
    {
        gameplayUIData.playerExperience.text = player.Experience + "/" + player.NextLevelExperience;
        gameplayUIData.playerExperienceBar.value = (float)player.Experience / player.NextLevelExperience;
    }

    public void UpdatePlayerGold()
    {
        gameplayUIData.playerGold.text = player.Gold.ToString();
    }
    #endregion

    #region - Level Information -
    public void UpdateWaveInfo(int currentWave, int totalWaveCount)
    {
        gameplayUIData.wave.text = currentWave + "/" + totalWaveCount;
    }

    public void UpdateEnergyInfo(int currentEnergy, int totalEnergy)
    {
        gameplayUIData.energy.text = currentEnergy + "/" + totalEnergy;
    }
    #endregion

    #region - Slots -
    /// <summary>
    /// Activates and deactivates the slots canvas.
    /// </summary>
    public void ToggleSlotsCanvas(bool isActive)
    {
        gameplayUIData.slotsCanvas.SetActive(isActive);
    }

    /// <summary>
    /// Empties all the slots by setting their Trap property to null.
    /// </summary>
    public void ClearSlots()
    {
        for (int i = 0; i < gameplayUIData.slots.Length - 1; i++)
        {
            gameplayUIData.slots[i].Trap = null;
        }
    }

    /// <summary>
    /// Sets up the slot at the specified index with the specified trap.
    /// </summary>
    /// <param name="slotIndex">Slot index value = [0, slots.length).</param>
    /// <param name="slotTrap">A trap for the slot or null if the slot should be empty.</param>
    public void SetUpSlot(int slotIndex, Trap slotTrap)
    {
        CheckSlotIndexValidity(slotIndex);

        gameplayUIData.slots[slotIndex].Trap = slotTrap;
    }

    /// <summary>
    /// Changes the currently active slot and activates its selection highlight. Deactivates highlight selection on all other slots (the previously active one).
    /// </summary>
    /// <param name="slotIndex">0 based index of the selected slot.</param>
    public void ChangeActiveSlot(int slotIndex)
    {
        if (activeSlotIndex != slotIndex)
        {
            CheckSlotIndexValidity(slotIndex);
            activeSlotIndex = slotIndex;

            for (int i = 0; i < gameplayUIData.slots.Length; i++)
            {
                if (i == slotIndex)
                {
                    gameplayUIData.slots[i].selectionHighlight.SetActive(true);
                }
                else
                {
                    gameplayUIData.slots[i].selectionHighlight.SetActive(false);
                }
            }
        }
    }

    private void CheckSlotIndexValidity(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > gameplayUIData.slots.Length - 1)
        {
            throw new UnityException("Slot index specified is invalid! Choose a number between 0 (inclusive) and the lenght of the available slots (exclusive).");
        }
    }

    // TODO When more traps and spells are added create a way to choose which ones are used in a level and a way to rearange the order of elements 
    #endregion

    #region - Enemy Health -
    /// <summary>
    /// Activates and deactivates the enemy health canvas.
    /// </summary>
    private void ToggleEnemyHealthCanvas(bool isActive)
    {
        gameplayUIData.enemyHealthCanvas.SetActive(isActive);
    }

    private void UpdateEnemyInfo(Enemy enemy)
    {
        UpdateEnemyHealth(enemy.currentHealth, enemy.MaxHealth);
        UpdateEnemyArmor(enemy.Armor);
    }

    private void UpdateEnemyHealth(float currentHealth, float maxHealth)
    {
        gameplayUIData.enemyHealth.text = (int)currentHealth + "/" + (int)maxHealth;
        gameplayUIData.enemyHealthBar.value = currentHealth / maxHealth;
    }

    private void UpdateEnemyArmor(int armor)
    {
        gameplayUIData.enemyArmor.text = armor.ToString();
    }
    #endregion

    #region - Gameplay Menu -
    /// <summary>
    /// Activates and deactivates the menu canvas.
    /// </summary>
    /// <param name="isActive"></param>
    public void ToggleMenuCanvas(bool isActive)
    {
        if (gameplayUIData == null)
        {
            print("null");
        }
        gameplayUIData.menuCanvas.SetActive(isActive);
    }

    /// <summary>
    /// Sets the specified menu panel to be the active panel and deactivates the other ones. Set to null to deactivate all menu panels.
    /// </summary>
    /// <param name="menuPanel">Menu panel to activate.</param>
    public void ShowMenuPanel(GameObject menuPanel)
    {
        gameplayUIData.levelStart.SetActive(menuPanel == gameplayUIData.levelStart);
        gameplayUIData.levelEnd.SetActive(menuPanel == gameplayUIData.levelEnd);
        gameplayUIData.pauseMenu.SetActive(menuPanel == gameplayUIData.pauseMenu);

        if (menuPanel != null && menuPanel != gameplayUIData.levelStart && menuPanel != gameplayUIData.levelEnd && menuPanel != gameplayUIData.pauseMenu)
        {
            throw new UnityException($"{menuPanel.name} isn't an element of Menu Canvas! Only menu panels can be passed as arguments to ShowMenupane method.");
        }
    }

    // Traps were chosen and the actual game can start // TODO Implement a system for choosing traps/spells in a level
    private void OnTrapsChosen()
    {
        ToggleMenuCanvas(false);
        ShowMenuPanel(null);
        ToggleCrosshair(true);

        GameManager.Instance.TogglePause(false, false);
    }

    private void ResetGameplayUI()
    {
        UpdatePlayerStats();

        // TODO until more traps and some spells are created and until the option of choosing what is to be used in a level is implemented
        // ui manager slots are set up from here - simply using all the traps the player has
        for (int i = 0; i < player.traps.Count; i++)
        {
            SetUpSlot(i, player.traps[i]);
        }

        ToggleMenuCanvas(true);
        ShowMenuPanel(gameplayUIData.levelStart);
        ToggleCrosshair(false);
    }

    private void SetUpGameplayMenuHandlers()
    {
        gameplayUIData.levelStartTestButton.onClick.AddListener(OnTrapsChosen);

        gameplayUIData.continueButton.onClick.AddListener(() => GameManager.Instance.TogglePause(false, true));
        //gameplayUIData.optionsButton.onClick.AddListener // TODO add options menu
        gameplayUIData.goToMainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1; // without this fade canvas won't allow changing of the scene
            SceneLoader.Instance.LoadMainMenu();
        });
    }

    private void GoToNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        // TODO From the data structure holding all the level scenes get the next one based on the current one and use SceneLoader to change scenes
    }

    /// <summary>
    /// Activates and deactivates pause menu panel in GameplayUI scene.
    /// </summary>
    private void TogglePauseMenu(bool isGamePaused)
    {
        ToggleMenuCanvas(isGamePaused);
        ToggleCrosshair(!isGamePaused);

        if (isGamePaused)
        {
            ShowMenuPanel(gameplayUIData.pauseMenu);
        }
        else
        {
            ShowMenuPanel(null);
        }
    }
    #endregion
    #endregion
    

    protected new void Awake()
    {
        InitializeSingleton();

        base.Awake();
    }

    private void Start()
    {
        player = Player.Instance;
        GameManager.Instance.PauseToggled += OnPauseToggled;
    }

    private void Update()
    {
        if (shouldTogglePauseMenu && gameplayUIData != null)
        {
            shouldTogglePauseMenu = false;
            TogglePauseMenu(GameManager.Instance.IsGamePaused);
        }

        print("Refactor this! Urgent!");
        // Next lines should only be executed if the wave has started
        cameraRay = playerCamera.ViewportPointToRay(viewportCenter);

        if (Physics.Raycast(cameraRay, out raycastHit, 25, raycastHitLayerMask)) // ADD TO CONST max distance?
        {
            if (raycastHit.transform.CompareTag("Enemy")) // ADD TO CONST
            {
                // Enemy was hit
                if (activeEnemy == null)
                {
                    // There was no active enemy previously so enemy health canvas is activating now
                    ToggleEnemyHealthCanvas(true);
                }

                activeEnemy = raycastHit.transform.GetComponent<Enemy>();

                if (activeEnemy == null)
                {
                    throw new UnityException($"{raycastHit.transform.gameObject.name} game object is tagged as Enemy, but it has no Enemy script attached.");
                }

                if (activeEnemy.isDead)
                {
                    activeEnemy = null;
                    ToggleEnemyHealthCanvas(false);
                }
            }
            else if (activeEnemy != null)
            {
                // Something other than enemy was hit
                activeEnemy = null;
                ToggleEnemyHealthCanvas(false);
            }
        }
        else if (activeEnemy != null)
        {
            // Nothing from the hit layer mask was hit
            activeEnemy = null;
            ToggleEnemyHealthCanvas(false);
        }

        if (activeEnemy != null)
        {
            UpdateEnemyInfo(activeEnemy);
        }
    }

    private void OnPauseToggled()
    {
        shouldTogglePauseMenu = true;
    }           
}

[System.Serializable]
public class Slot
{
    public Image image;
    public Text goldCost;
    public GameObject selectionHighlight;

    private Trap trap;

    public Trap Trap
    {
        get
        {
            return trap;
        }
        set
        {
            trap = value;

            if (trap != null)
            {
                image.sprite = trap.thumbnail;
                goldCost.text = trap.Cost.ToString();
            }
            else
            {
                // Slot is empty
                image.sprite = UIManager.Instance.emptySlotSprite;
                goldCost.text = string.Empty;
            }

            selectionHighlight.SetActive(false);
        }
    }

    public void EmptySlot()
    {
        this.Trap = null;
    }
}
