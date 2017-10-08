using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceablePool : PoolBase<PlaceableType, Placeable>
{    
    [Range(0, 100)]
    [SerializeField]
    private int placeableStartCount;
    [Space(10)]
    [SerializeField] private List<PlaceablePoolPair> placeablePools;

    #region - "Singleton" Instance -
    private static PlaceablePool instance;

    public static PlaceablePool Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling PlaceablePool.Instance before it is set! Change script execution order.");
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

    protected override void Awake()
    {
        InitializeSingleton();

        base.Awake();
        SetAllPools();
    }

    protected override void SetAllPools()
    {
        for (int i = 0; i < placeablePools.Count; i++)
        {
            allPools.Add(placeablePools[i].pool, placeablePools[i].poolObject);
        }
    }

    protected override void InitializeDefaultActivePools()
    {
        // Go through each pool and instantiate a number of objects for it
        for (int i = 0; i < placeablePools.Count; i++)
        {
            ExpandPool(placeablePools[i].pool, placeableStartCount);
        }
    }

    private new void Update()
    {
        base.Update();
    }
}


[System.Serializable]
public struct PlaceablePoolPair
{
    public PlaceableType pool;
    public Placeable poolObject;
}
