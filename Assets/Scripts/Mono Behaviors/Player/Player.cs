using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<Spell> spells;
    public List<Trap> traps;
    public Inventory inventory;

    //private int gold;
    //private int experience;
    //private int level;

    public event Action LevelUp;

    public int Gold { get; private set; }       // TODO Add an event for when Gold is incread
    public int Experience { get; private set; } // TODO Add an event for when XP is increased
    public int Level { get; private set; }      // TODO Add an event for when Level is increased
    public int NextLevelExperience
    {
        get
        {
            return (Level + 1) * Level * 500;
        }
    }

    #region - "Singleton" Instance -
    private static Player instance;

    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling Player.Instance before it is set!.");
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

            GameManager.Instance.Player = this;
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }
    #endregion

    private void Awake()
    {
        InitializeSingleton(); 

        // TODO Get player data from saved file if it exists
        Level = 2;
        Experience = 1325;
        Gold = 750;
    }

    public void GainExperience(int xpAmount)
    {
        //if (xpAmount < 0)
        //{
        //    // Currently it's not allowed to reduce xp
        //    return;
        //}

        //experience += xpAmount;

        //while (experience >= NextLevelExperience)
        //{
        //     IncreaseLevel();
        //}
    }
    
    /// <summary>
    /// Increases the level by the specified amount. 
    /// </summary>
    private void IncreaseLevel(int increaseAmount = 1)
    {
        if (increaseAmount < 0)
        {
            // Currently it's not allowed to decrease a level
            return;
        }

        Level += increaseAmount;
        LevelUp?.Invoke();
    }
}
