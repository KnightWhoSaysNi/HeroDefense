using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int gold;
    public List<Spell> spells;
    public List<Trap> traps;
    public Inventory inventory;

    [SerializeField] private int experience;

    public event Action LevelUp;

    public int Level { get; private set; }
    public int NextLevelExperience
    {
        get
        {
            return (Level + 1) * Level * 500;
        }
    }

    private void Start()
    {
        // TODO Get player data from saved file if it exists
    }

    public void GainExperience(int xpAmount)
    {
        if (xpAmount < 0)
        {
            // Currently it's not allowed to reduce xp
            return;
        }

        experience += xpAmount;

        while (experience >= NextLevelExperience)
        {
             IncreaseLevel();
        }
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
