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

    // ADD TO CONST
    private readonly string levelStartMessage = "Press G to start";
    private readonly string waveStartedMessage = "Wave started";
    private readonly string finalWaveMessage = "Final wave";
    /// <summary>
    /// Amount of seconds to keep the message visible
    /// </summary>
    private readonly float defaultMessageAliveTime = 4;

    private bool isRightClickRegistered;

    private bool isPauseToggled;
    private bool shouldRaycastForInformation;

    private bool shouldShowCountdown;
    private bool isFinalWave;
    private bool shouldShowMessage;
    private float countdownTimer;
    private float countdownWholeNumber;
    private float messageTimer;

    private int activeSlotIndex;
    private Enemy activeEnemy;
    private Placeable activePlaceable;
    private Player player;

    public static event Action LevelStarted;

    /// <summary>
    /// Returns true only if gameplayUIData has been received and UIManager can manipulate UI elements of the GameplayUI scene.
    /// </summary>
    public bool IsReadyToReceiveUpdates // TODO See who else can benefit from using this
    {
        get
        {
            return gameplayUIData != null;
        }
    }
    private Placeable ActivePlaceable
    {
        get
        {
            return activePlaceable;
        }
        set
        {
            if (activePlaceable != value)
            {
                // Deactivate current active placeable's range visual before setting the new active placeable
                if (activePlaceable != null)
                {
                    activePlaceable.rangeVisual.SetActive(false);
                }

                activePlaceable = value;
            }
        }
    }

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
            Destroy(this.gameObject);
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

    /// <summary>
    /// Activates/deactivates the crosshair.
    /// </summary>
    public void CrosshairSetActive(bool isActive)
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
        mainMenuUIData.tutorialLevel.onClick.AddListener(SceneLoader.Instance.LoadTutorialLevel);
    }

    #endregion

    #region - Gameplay UI -

    public void ResetGameplayUI()
    {
        PrepareForSceneChange(false);

        CrosshairSetActive(false);
        gameplayUIData.enemyCanvas.SetActive(false);
        gameplayUIData.placeableCanvas.SetActive(false);
        gameplayUIData.messagingCanvas.SetActive(false);
        gameplayUIData.menuCanvas.SetActive(true);

        ShowMenuPanel(gameplayUIData.levelStartPanel);
        ChangeActiveSlot(0);

        UpdatePlayerStats();

        // TODO until more traps and some spells are created and until the option of choosing what is to be used in a level is implemented
        // ui manager slots are set up from here - simply using all the traps the player has
        for (int i = 0; i < player.traps.Count; i++)
        {
            SetUpSlot(i, player.traps[i]);
        }
    }

    private void SetUpGameplayMenuHandlers()
    {
        gameplayUIData.levelStartTest.onClick.AddListener(OnTrapsChosen);

        gameplayUIData.levelEndGoToMainMenu.onClick.AddListener(GoToMainMenu);
        gameplayUIData.levelEndRestart.onClick.AddListener(Restart);
        gameplayUIData.levelEndGoToNextLevel.onClick.AddListener(() => { print("NOT IMPLEMENTED!"); GoToNextLevel(); });

        gameplayUIData.pauseContinue.onClick.AddListener(() => GameManager.Instance.ChangePauseState(false, true));
        gameplayUIData.pauseRestart.onClick.AddListener(Restart);
        gameplayUIData.pauseGoToMainMenu.onClick.AddListener(GoToMainMenu);
        gameplayUIData.pauseQuit.onClick.AddListener(Quit); 
    }

    private void Restart()
    {
        ResetGameplayUI();
        gameplayUIData.menuGraphicRaycaster.enabled = true;

        LevelManager.Instance.RestartLevel();

        shouldRaycastForInformation = true; 
    }

    private void GoToMainMenu()
    {
        PrepareForSceneChange(true);
        SceneLoader.Instance.LoadMainMenu();
    }

    /// <summary>
    /// Sets certain values to allow for a scene change.
    /// </summary>
    private void PrepareForSceneChange(bool shouldChangeTimeScale)
    {
        PlaceablePool.Instance.ReclaimAllObjects();
        EnemyPool.Instance.ReclaimAllObjects();

        shouldRaycastForInformation = false;
        shouldShowCountdown = false;
        shouldShowMessage = false;

        if (shouldChangeTimeScale)
        {
            // Canvas fading won't allow changing the scene if time scale isn't 1
            Time.timeScale = 1;
        }
    }

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
        if (gameplayUIData != null)
        {
            gameplayUIData.playerGold.text = player.Gold.ToString();
        }
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
    public void SlotsCanvasSetActive(bool isActive)
    {
        gameplayUIData.slotsCanvas.SetActive(isActive);
    }

    /// <summary>
    /// Change the currently active slot and activate its selection highlight. Deactivates highlight selection the previously active slot.
    /// </summary>
    /// <param name="slotIndex">0 based index of the selected slot.</param>
    public void ChangeActiveSlot(int slotIndex)
    {
        if (activeSlotIndex != slotIndex)
        {
            activeSlotIndex = slotIndex;

            int numberOfUsedSlots = gameplayUIData.slots.Length;

            for (int i = 0; i < numberOfUsedSlots; i++)
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

    /// <summary>
    /// Empties all the slots by setting their Trap property to null.
    /// </summary>
    private void ClearSlots()
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
    private void SetUpSlot(int slotIndex, Trap slotTrap)
    {
        gameplayUIData.slots[slotIndex].Trap = slotTrap;
    }

    // TODO When more traps and spells are added create a way to choose which ones are used in a level and a way to rearange the order of elements 
    #endregion

    #region - Enemy Information -

    /// <summary>
    /// Displays active enemy's info in the enemy canvas.
    /// </summary>
    private void UpdateEnemyInfo()
    {
        UpdateEnemyHealth(activeEnemy.CurrentHealth, activeEnemy.MaxHealth);
        UpdateEnemyArmor(activeEnemy.Armor);
    }

    /// <summary>
    /// Updates the displayed enemy's health text and health bar value.
    /// </summary>
    private void UpdateEnemyHealth(float currentHealth, float maxHealth)
    {
        gameplayUIData.enemyHealth.text = (int)currentHealth + "/" + (int)maxHealth;
        gameplayUIData.enemyHealthBar.value = currentHealth / maxHealth;
    }

    /// <summary>
    /// Updates the displayed enemy's armor.
    /// </summary>
    private void UpdateEnemyArmor(int armor)
    {
        gameplayUIData.enemyArmor.text = armor.ToString();
    }
    #endregion

    #region - Placeable Information -

    /// <summary>
    /// Displays the current placeable's sell price.
    /// </summary>
    private void DisplayPlaceableInfo()
    {
        int sellPrice = PlacementManager.Instance.GetSellPrice(activePlaceable);
        gameplayUIData.placeableSellPrice.text = sellPrice + " gold";
    }

    #endregion

    #region - Gameplay Menu -

    /// <summary>
    /// Sets the specified menu panel to be the active panel and deactivates the other ones. Set to null to deactivate all menu panels.
    /// </summary>
    /// <param name="menuPanel">Menu panel to activate.</param>
    private void ShowMenuPanel(GameObject menuPanel)
    {
        gameplayUIData.levelStartPanel.SetActive(menuPanel == gameplayUIData.levelStartPanel);
        gameplayUIData.levelEndPanel.SetActive(menuPanel == gameplayUIData.levelEndPanel);
        gameplayUIData.pauseMenuPanel.SetActive(menuPanel == gameplayUIData.pauseMenuPanel);

        if (menuPanel != null && menuPanel != gameplayUIData.levelStartPanel && menuPanel != gameplayUIData.levelEndPanel && menuPanel != gameplayUIData.pauseMenuPanel)
        {
            throw new UnityException($"{menuPanel.name} isn't an element of Menu Canvas! Only menu panels can be passed as arguments to {nameof(ShowMenuPanel)} method.");
        }
    }

    // Traps were chosen and the actual game can start // TODO Implement a system for choosing traps/spells in a level
    private void OnTrapsChosen()
    {
        gameplayUIData.menuCanvas.SetActive(false);
        ShowMenuPanel(null);
        CrosshairSetActive(true);
        ShowLevelStartMessage();

        LevelStarted?.Invoke();
        GameManager.Instance.ChangePauseState(false, false);        
    }

    private void GoToNextLevel()
    {
        PrepareForSceneChange(true);

        // TODO From the data structure holding all the level scenes get the next one based on the current one and use SceneLoader to change scenes
        string currentScene = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Activates and deactivates pause menu panel in GameplayUI scene. 
    /// </summary>
    private void PauseMenuSetActive(bool isActive)
    {
        gameplayUIData.menuCanvas.SetActive(isActive);
        CrosshairSetActive(!isActive);

        if (isActive)
        {
            ShowMenuPanel(gameplayUIData.pauseMenuPanel);
        }
        else
        {
            ShowMenuPanel(null);
        }
    }
    #endregion

    #region - Messaging system -
    /// <summary>
    /// Displayes the message in the messaging canvas for the specified time in seconds.
    /// </summary>
    /// <param name="message">Message to display.</param>
    /// <param name="messageAliveTime">Amount of time in seconds to display the message.</param>
    public void ShowMessage(string message, float messageAliveTime)
    {
        shouldShowMessage = true;
        messageTimer = messageAliveTime;

        gameplayUIData.message.text = message;
        gameplayUIData.messagingCanvas.SetActive(true);
    }

    /// <summary>
    /// Displays a countdown from the specified start seconds to zero, after which wave started message is shown.
    /// </summary>
    public void ShowCountdown(float countdownSeconds, bool isFinalWave = false)
    {
        if (countdownSeconds < 0)
        {
            print("Countdown from negative makes no sense. Check the caller.");
            return;
        }

        this.isFinalWave = isFinalWave;

        gameplayUIData.messagingCanvas.SetActive(true);
        shouldShowCountdown = true;
        countdownTimer = countdownSeconds;
    }

    private void ShowLevelStartMessage()
    {
        gameplayUIData.messagingCanvas.SetActive(true);
        gameplayUIData.message.text = levelStartMessage;
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
        playerCamera = GameManager.Instance.playerCamera;

        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.PauseStateChanged += OnPauseChanged;
        PlacementManager.PlacementModeChanged += OnPlacementModeChanged;
        LevelManager.PlayerWon += () => OnGameEnd(true);
        LevelManager.PlayerLost += () => OnGameEnd(false);
    }

    private void Update()
    {
        CheckForGameplayPause();

        if (shouldRaycastForInformation)
        {
            RaycastForInformation();

            if (activePlaceable != null)
            {
                CheckForPlaceableSell();
            }
        }

        // Range visual is only shown on right click
        if (Input.GetMouseButton(1))
        {
            activePlaceable?.rangeVisual.SetActive(true);
        }

        CheckForCountdownMessage();
        CheckForKeepMessageAlive();
    }

    /// <summary>
    /// Checks for gameplay pause/unpause and shows/hides the pause menu accordingly.
    /// </summary>
    private void CheckForGameplayPause()
    {
        if (isPauseToggled && gameplayUIData != null)
        {
            isPauseToggled = false;
            PauseMenuSetActive(GameManager.Instance.IsGamePaused);
        }
    }    

    /// <summary>
    /// Raycasts from the camera through the center of the viewport and if enemy or placeable were hit displays their information.
    /// </summary>
    private void RaycastForInformation()
    {
        cameraRay = playerCamera.ViewportPointToRay(viewportCenter);

        // A LOT of "else if" checks are used so that no unnecessary canvas activation/deactivation happens with each frame
        if (Physics.Raycast(cameraRay, out raycastHit, 25, raycastHitLayerMask)) // ADD TO CONST max distance
        {
            if (raycastHit.transform.CompareTag("Enemy")) // ADD TO CONST
            {
                // Enemy is hit
                ActivePlaceable = null;
                UpdateActiveEnemy();
            }
            else if (raycastHit.transform.CompareTag("Placeable") || raycastHit.transform.CompareTag("Sellable")) // ADD TO CONST
            {
                // Placeable is hit
                activeEnemy = null;
                UpdateActivePlaceable();
            }
            else if (activeEnemy != null)
            {
                // Something other than enemy or placeable is hit now, but last frame enemy was hit
                activeEnemy = null;
                gameplayUIData.enemyCanvas.SetActive(false);
            }
            else if (activePlaceable != null)
            {
                // Something other than enemy or placeable is hit now, but last frame placeable was hit
                ActivePlaceable = null;
                gameplayUIData.placeableCanvas.SetActive(false);
            }
        }
        else if (activeEnemy != null)
        {
            // Nothing is hit now, but last frame enemy was hit
            activeEnemy = null;
            gameplayUIData.enemyCanvas.SetActive(false);
        }
        else if (activePlaceable != null)
        {
            // Nothing is hit now, but last frame placeable was hit
            ActivePlaceable = null;
            gameplayUIData.placeableCanvas.SetActive(false);
        }

        if (activeEnemy != null)
        {
            UpdateEnemyInfo();
        }
        else if (activePlaceable != null)
        {
            DisplayPlaceableInfo();
        }
    }

    /// <summary>
    /// Finds and updates active enemy from the raycastHit object tagged as Enemy.
    /// </summary>
    private void UpdateActiveEnemy()
    {        
        if (activeEnemy == null)
        {
            // There was no active enemy previously so enemy health canvas is activating now
            gameplayUIData.enemyCanvas.SetActive(true);
            gameplayUIData.placeableCanvas.SetActive(false);
        }

        activeEnemy = raycastHit.transform.GetComponent<Enemy>();

        if (activeEnemy == null)
        {
            throw new UnityException($"{raycastHit.transform.gameObject.name} game object is tagged as Enemy, but it has no Enemy script attached.");
        }

        // In case the enemy is dead but the object is still in the scene
        if (activeEnemy.IsDead)
        {
            activeEnemy = null;
            gameplayUIData.enemyCanvas.SetActive(false);
        }
    }

    /// <summary>
    /// Finds and updates active placeable from the raycastHit object tagged as Placeable.
    /// </summary>
    private void UpdateActivePlaceable()
    {
        if (activePlaceable == null)
        {
            // There was no active placeable previously so placeable canvas is activating now
            gameplayUIData.enemyCanvas.SetActive(false);
            gameplayUIData.placeableCanvas.SetActive(true);
        }

        ActivePlaceable = raycastHit.transform.GetComponent<Placeable>();        
    }

    /// <summary>
    /// If 'E' was pressed reports that the active placeable should be sold.
    /// </summary>
    private void CheckForPlaceableSell()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PlacementManager.Instance.SellPlaceable(activePlaceable);            

            gameplayUIData.placeableCanvas.SetActive(false);
            ActivePlaceable = null;
        }
    }

    /// <summary>
    /// If flagged as needed displays the countdown to 0, in whole numbers, and then shows the wave started message for a while.
    /// </summary>
    private void CheckForCountdownMessage()
    {
        // Until the countdown reaches 0 decrease it and display whole numbers
        if (shouldShowCountdown && countdownTimer >= 0)
        {
            countdownWholeNumber = Mathf.Ceil(countdownTimer);
            if (gameplayUIData.message.text != countdownWholeNumber.ToString())
            {
                gameplayUIData.message.text = countdownWholeNumber.ToString();
            }

            if (countdownTimer == 0)
            {
                shouldShowCountdown = false;

                ShowMessage(isFinalWave ? finalWaveMessage : waveStartedMessage, defaultMessageAliveTime);
            }

            countdownTimer -= Time.deltaTime;
            if (countdownTimer < 0)
            {
                countdownTimer = 0;
            }
        }
    }

    /// <summary>
    /// If a message is displayed this method will keep it visible until the messageTimer reaches zero (it's being decreased every frame).
    /// </summary>
    private void CheckForKeepMessageAlive()
    {
        if (shouldShowMessage && messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;

            if (messageTimer <= 0)
            {
                shouldShowMessage = false;
                gameplayUIData.messagingCanvas.SetActive(false);
            }
        }
    }

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        if (loadedScene.name == "MainMenu") // ADD TO CONST
        {
            // GameplayUIData holds only nulls, because GameplayUI scene was removed. 
            // IsReadyToReceiveUpdates property needs to return false and that is assured by setting gameplayUIData to null
            gameplayUIData = null;
        }
    }

    /// <summary>
    /// Signals that pause/unpause was registered in game manager.
    /// </summary>
    private void OnPauseChanged()
    {
        isPauseToggled = true;
    }

    private void OnPlacementModeChanged(bool isInPlacementMode)
    {
        shouldRaycastForInformation = !isInPlacementMode;

        if (isInPlacementMode)
        {
            activeEnemy = null;
            ActivePlaceable = null;
            gameplayUIData.enemyCanvas.SetActive(false);
            gameplayUIData.placeableCanvas.SetActive(false);
        }
    }

    /// <summary>
    /// Displays appropriate screen for the player depending on whether he won or lost the game.
    /// </summary>
    private void OnGameEnd(bool hasPlayerWon)
    {
        PrepareForSceneChange(false);

        CrosshairSetActive(false);

        gameplayUIData.enemyCanvas.SetActive(false);
        gameplayUIData.placeableCanvas.SetActive(false);
        gameplayUIData.messagingCanvas.SetActive(false);

        gameplayUIData.winText.SetActive(hasPlayerWon);
        gameplayUIData.loseText.SetActive(!hasPlayerWon);

        ShowMenuPanel(gameplayUIData.levelEndPanel);
        gameplayUIData.menuCanvas.SetActive(true);

        if (hasPlayerWon)
        {
            //gameplayUIData.levelEndGoToNextLevel.interactable = true;
        }
        else
        {
            gameplayUIData.levelEndGoToNextLevel.interactable = false;
        }
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

            //selectionHighlight.SetActive(false);
        }
    }

    public void EmptySlot()
    {
        this.Trap = null;
    }
}
