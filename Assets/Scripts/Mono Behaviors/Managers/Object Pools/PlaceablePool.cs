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
    [Tooltip("Placeable objects for which you wish to make a pool. NO DUPLICATES ALLOWED! WILL THROW EXCEPTION!")]
    [SerializeField]
    private Placeable[] placeablePools;
    private HashSet<Placeable> placeablePoolsSet;

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

    /// <summary>
    /// Return the specified placeable to its pool.
    /// </summary>
    public override void ReclaimObject(Placeable placeableToReclaim)
    {
        base.ReclaimObject(placeableToReclaim.PlaceableType, placeableToReclaim, true);
    }

    protected override void Awake()
    {
        InitializeSingleton();

        placeablePoolsSet = new HashSet<Placeable>();
        PopulateSet();

        base.Awake();
        SetAllPools();
    }

    protected override void SetAllPools()
    {
        foreach (Placeable placeable in placeablePoolsSet)
        {
            allPools.Add(placeable.PlaceableType, placeable);
        }
    }

    protected override void InitializeDefaultActivePools()
    {
        // Go through each pool and instantiate a number of objects for it
        foreach (Placeable placeable in placeablePoolsSet)
        {
            ExpandPool(placeable.PlaceableType, placeableStartCount);
        }
    }    

    /// <summary>
    /// Populates a hash set of placeables to make sure there are no duplicates given in the inspector.
    /// </summary>
    private void PopulateSet()
    {
        bool isAdded;
        for (int i = 0; i < placeablePools.Length; i++)
        {
            isAdded = placeablePoolsSet.Add(placeablePools[i]);

            if (!isAdded)
            {
                throw new UnityException($"Placeable pool has a duplicate placeable. A pool for {placeablePools[i].PlaceableType} already exists. Check the inspector.");
            }
        }
    }
}