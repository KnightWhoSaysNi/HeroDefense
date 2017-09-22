using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlacementManager placementManager;
    private LevelManager levelManager;
    public UIManager uiManager;

    private bool isInActiveLevel;

    #region - "Singleton" Instance -
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    throw new UnityException("GameObject missing!");
                }
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

    private void Awake()
    {
        //InitializeSingleton();

        placementManager = GetComponent<PlacementManager>();
    }

    private void Start()
    {
        SceneManager.LoadScene("GameplayUI", LoadSceneMode.Additive);

        StartCoroutine(FindUIManager());
    }

    private IEnumerator FindUIManager()
    {
        // A frame must pass before newly added additive scene can yield back its objects
        yield return null;

        GameObject uiManagerObject = GameObject.FindGameObjectWithTag("UIManager");

        if (uiManagerObject != null)
        {
            uiManager = uiManagerObject.GetComponent<UIManager>();
        }

        print(uiManager.gameObject.name);
    }

    private void Update()
    {
        
    }
}
