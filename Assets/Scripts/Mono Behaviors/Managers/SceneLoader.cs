using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;
    public float fadeSpeed = 2f;

    private readonly string mainMenuScene = "MainMenu"; // ADD TO CONST
    private readonly string gameplayUIScene = "GameplayUI"; // ADD TO CONST

    private bool isGameplayUILoaded;

    public static event System.Action LevelLoaded;

    #region - "Singleton" Instance -
    private static SceneLoader instance;

    public static SceneLoader Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling SceneLoader.Instance before it is set!.");
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
    /// Loads a scene with the specified name in single scene mode. Additively loads gameplay UI scene if necessary.
    /// </summary>
    public void LoadScene(string sceneName)
    {        
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// Used for transition between an active level and the main menu.
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }

    private void Awake()
    {
        InitializeSingleton();
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Fade out
        while (fadeCanvasGroup.alpha < 1)
        {
            fadeCanvasGroup.alpha += (fadeSpeed * Time.deltaTime);
            yield return null;
        }

        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!loadingOperation.isDone)
        {
            yield return null;
        }
        
        if (sceneName == mainMenuScene)
        {
            // Went back from a level to the main scene so gameplayUI scene was unloaded
            isGameplayUILoaded = false;
        }
        else
        {
            // Level scene loaded, either from the main menu or from some other level
            LevelLoaded?.Invoke();

            if (!isGameplayUILoaded)
            {
                // Level scene loaded but gameplay UI is not, so loading it now
                SceneManager.LoadSceneAsync(gameplayUIScene, LoadSceneMode.Additive);
                isGameplayUILoaded = true;
            }
        }


        // Fade in
        while (fadeCanvasGroup.alpha > 0)
        {
            fadeCanvasGroup.alpha -= (fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
