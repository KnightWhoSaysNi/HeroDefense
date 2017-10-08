using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<Spell> spells;
    public List<Trap> traps;
    public Inventory inventory;

    private int gold;
    private int experience;
    private int level;

    public static event Action LevelGained;
    public static event Action ExperienceGained;
    public static event Action GoldGained;

    public int Gold
    {
        get
        {
            return gold;
        }
        set
        {
            gold = value;

            if (gold < 0)
            {
                gold = 0;
            }

            GoldGained?.Invoke();
        }
    }
    public int Experience
    {
        get
        {
            return experience;
        }
        set
        {
            experience = value;

            if (experience < 0)
            {
                experience = 0;
            }

            while (experience >= NextLevelExperience)
            {
                Level++;
            }

            ExperienceGained?.Invoke();
        }
    }
    public int Level
    {
        get
        {
            return level;
        }
        private set
        {
            level = value;

            if (level < 1)
            {
                level = 1;
            }

            LevelGained?.Invoke();
        }
    }
    public int NextLevelExperience
    {
        get
        {
            return (Level + 1) * Level * 500;
        }
    }

    #region - "Singleton" Instance -
    private static Player instance;
    private static bool isBeingDisabled;

    public static Player Instance
    {
        get
        {
            if (instance == null && !isBeingDisabled)
            {
                throw new UnityException("Someone is calling Player.Instance before it is set! Change execution order.");
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
        InitializeSingleton();
        level = 1;
    }

    private void Start()
    {        
    }

    private void OnDisable()
    {
        // Certain scripts call Player.Instance in their OnDisable methods. ApplicationQuit disables Player before disabling those scripts.
        // This prevents a false positive exception throwing for trying to access Player.Instance before it has been set
        isBeingDisabled = true;
    }
}
