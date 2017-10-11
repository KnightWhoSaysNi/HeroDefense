
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
    [SerializeField]
    private List<EnemyPoolPair> enemyPools;

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

        base.Awake();
        SetAllPools();
    }

    protected override void SetAllPools()
    {
        for (int i = 0; i < enemyPools.Count; i++)
        {
            allPools.Add(enemyPools[i].pool, enemyPools[i].poolObject);
        }
    }

    protected override void InitializeDefaultActivePools()
    {
        // Go through each pool and instantiate a number of objects for it
        for (int i = 0; i < enemyPools.Count; i++)
        {
            ExpandPool(enemyPools[i].pool, enemyStartCount);
        }
    }    
}

[System.Serializable]
public struct EnemyPoolPair
{
    public EnemyType pool;
    public Enemy poolObject;
}
