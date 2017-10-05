using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;
    public float fadeSpeed = 2f;

    private bool isGameplayUILoaded;

    public event System.Action MainMenuLoading;

    public bool IsGameplayUILoaded
    {
        get
        {
            return isGameplayUILoaded;
        }
    }

    #region - "Singleton" Instance -
    private static SceneLoader instance;

    public static SceneLoader Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling SceneLoader.Instance before it is set! Change script execution order.");
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
    /// Loads a scene with the specified name load scene mode. If the scene is a level scene additively loads GameplayUI scene, unless it's already loaded.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StopAllCoroutines();
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// Used for transition between an active level and the main menu.
    /// </summary>
    /// <remarks>Loads in single mode.</remarks>
    public void LoadMainMenu()
    {
        MainMenuLoading?.Invoke();
        LoadScene(Constants.MainMenuSceneName);
    }

    /// <summary>
    /// Loads the tutorial level.
    /// </summary>
    /// <remarks>Uses additive load scene mode.</remarks>
    public void LoadTutorialLevel()
    {
        LoadScene(Constants.TutorialSceneName);
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

        AsyncOperation loadingOperation; 

        if (isGameplayUILoaded && sceneName != Constants.MainMenuSceneName)
        {
            // Going from level to level, so unload the current scene and additively load the new one
            SceneManager.UnloadSceneAsync(0);
            loadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
        else
        {
            // Going from or to MainMenu scene 
            loadingOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        while (!loadingOperation.isDone)
        {
            yield return null;
        }
        
        if (sceneName == Constants.MainMenuSceneName)
        {
            // Went back from a level to the main scene so gameplayUI scene was unloaded
            isGameplayUILoaded = false;
        }
        else // Level scene loaded
        {            
            if (isGameplayUILoaded)
            {
                // GameplayUI scene is already loaded, so change the level scene to be the active one
                Scene loadedLevel = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                SceneManager.SetActiveScene(loadedLevel);
            }
            else
            {
                // Went from MainMenu to the level scene so loading the GameplayUI additively
                SceneManager.LoadSceneAsync(Constants.GameplayUISceneName, LoadSceneMode.Additive);
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
