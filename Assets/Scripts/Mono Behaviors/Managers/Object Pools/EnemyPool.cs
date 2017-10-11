
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : PoolBase<EnemyType, Enemy>
{
    [Range(0, 200)]
    [SerializeField]
    private int enemyStartCount;
    [Space(10)]
    [Tooltip("Enemy objects for which you wish to make a pool.")]
    [SerializeField]
    private Enemy[] enemyPools;
    private HashSet<Enemy> enemyPoolsSet;

    #region - "Singleton" Instance -
    private static EnemyPool instance;

    public static EnemyPool Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling EnemyPool.Instance before it is set! Change script execution order.");
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
    /// Return the specified enemy to its pool.
    /// </summary>
    public override void ReclaimObject(Enemy enemyToReclaim)
    {
        base.ReclaimObject(enemyToReclaim.EnemyType, enemyToReclaim, true);
    }

    protected override void Awake()
    {
        InitializeSingleton();

        enemyPoolsSet = new HashSet<Enemy>();
        PopulateSet();

        base.Awake();
        SetAllPools();
    }

    protected override void SetAllPools()
    {
        foreach (Enemy enemy in enemyPoolsSet)
        {
            allPools.Add(enemy.EnemyType, enemy);
        }
    }

    protected override void InitializeDefaultActivePools()
    {
        // Go through each pool and instantiate a number of objects for it
        foreach (Enemy enemy in enemyPoolsSet)
        {
            ExpandPool(enemy.EnemyType, enemyStartCount);
        }
    }

    /// <summary>
    /// Populates a hash set of enemies to make sure there are no duplicates given in the inspector.
    /// </summary>
    private void PopulateSet()
    {
        bool isAdded;
        for (int i = 0; i < enemyPools.Length; i++)
        {
            isAdded = enemyPoolsSet.Add(enemyPools[i]);

            if (!isAdded)
            {
                throw new UnityException($"Enemy pool has a duplicate enemy. A pool for {enemyPools[i].EnemyType} already exists. Check the inspector.");
            }
        }
    }
}
