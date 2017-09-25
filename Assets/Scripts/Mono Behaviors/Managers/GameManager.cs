using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Camera playerCamera;
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController playerController;
    private Player player;

    private PlacementManager placementManager;
    private LevelManager levelManager;
    private UIManager uiManager;

    private bool canPauseGame;

    public event Action PauseToggled;

    public Player Player
    {
        get
        {
            return player;
        }
        set
        {
            player = value;

            if (player != null)
            {
                // TODO Do something here or delete the property
            }
        }
    }      
    public bool IsGamePaused { get; private set; }

    #region - "Singleton" Instance -
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling GameManager.Instance before it is set! Change script execution order.");                
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

            InitializeGameManager();
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }
    #endregion    

    // TEST TODO Write this correctly
    public void TogglePause(bool isPaused, bool shouldRaiseEvent)
    {
        IsGamePaused = isPaused;
        if (shouldRaiseEvent)
        {
            PauseToggled?.Invoke();
        }

        playerController.enabled = !isPaused;
        placementManager.enabled = !isPaused;
        Cursor.visible = isPaused;

        if (isPaused)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Update()
    {        
        // If #if UNITY_EDITOR is used the code in #else region has no IntelliSense so this alternative is used
        if (Application.isEditor) 
        {
            if (Input.GetKeyDown(KeyCode.P) && canPauseGame)
            {
                TogglePause(!IsGamePaused, true);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canPauseGame)
            {
                TogglePause(!IsGamePaused, true);
            }
        }        
    }

    private void InitializeGameManager()
    {
        levelManager = GetComponent<LevelManager>();
        placementManager = GetComponent<PlacementManager>();
        uiManager = GetComponent<UIManager>();

        placementManager.playerCamera = playerCamera;
        uiManager.playerCamera = playerCamera;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode)
    {
        if (loadedScene.name == "MainMenu") // ADD TO CONST
        {
            playerController.enabled = false;
            placementManager.enabled = false;

            canPauseGame = false;
        }
        else if (loadedScene.name != "GameplayUI") // ADD TO CONST 
        {
            // Level scene loaded
            Player.transform.position = LevelManager.PlayerStartBearings.position;
            Player.transform.rotation = LevelManager.PlayerStartBearings.rotation;

            if (!IsGamePaused)
            {
                TogglePause(true, false); // TODO
            }

            canPauseGame = true;
        }
    }    
}
