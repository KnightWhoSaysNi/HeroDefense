using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region - Fields -
    public Camera playerCamera;
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController playerController;

    private bool canPauseGame;
    #endregion

    #region - Events -
    public static event Action PauseStateChanged;
    #endregion

    #region - Properties -
    public bool IsGamePaused { get; private set; } 
    #endregion

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
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

    #region - Public methods -
    /// <summary>
    /// Goes in and out of paused state, enabling and disabling required components. Raises the PauseStateChanged event if so specified.
    /// </summary>
    public void ChangePauseState(bool isPaused, bool shouldRaiseEvent)
    {
        IsGamePaused = isPaused;
        if (shouldRaiseEvent)
        {
            PauseStateChanged?.Invoke();
        }

        playerController.enabled = !isPaused;
        PlacementManager.Instance.enabled = !isPaused;
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
    #endregion

    #region - MonoBehavior methods -
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UIManager.LevelStarted += () => OnLevelStarted(false);
        LevelManager.LevelRestarted += () => OnLevelStarted(true);
        LevelManager.PlayerWon += OnGameEnd;
        LevelManager.PlayerLost += OnGameEnd;

        PlacementManager.Instance.playerCamera = playerCamera;
    }

    private void Update()
    {
        // If #if UNITY_EDITOR is used the code in #else region has no IntelliSense so this alternative is used
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.P) && canPauseGame)
            {
                ChangePauseState(!IsGamePaused, true);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canPauseGame)
            {
                ChangePauseState(!IsGamePaused, true);
            }
        }
    }
    #endregion

    #region - Event handlers -
    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode loadedSceneMode)
    {
        if (loadedScene.name == Constants.MainMenuSceneName)
        {
            playerController.enabled = false;
            PlacementManager.Instance.enabled = false;

            canPauseGame = false;
        }
        else if (loadedScene.name != Constants.GameplayUISceneName)
        {
            // Level scene loaded
            if (!IsGamePaused)
            {
                ChangePauseState(true, false);
            }

            canPauseGame = false;
        }
    }

    private void OnLevelStarted(bool isLevelRestarted)
    {
        canPauseGame = !isLevelRestarted;
    }

    private void OnGameEnd()
    {
        canPauseGame = false;
        ChangePauseState(true, false);
    } 
    #endregion
}
