using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PoolBase<TPool, TPoolObject> : MonoBehaviour    
    where TPool : ScriptableObject 
    where TPoolObject : MonoBehaviour, IPoolable
{
    /// <summary>
    /// All different prefabs for which object pools are created.
    /// </summary>
    protected Dictionary<TPool, TPoolObject> allPools;
    /// <summary>
    /// Minimum count of objects a pool can have at which point it must expand and instantiate new objects.
    /// </summary>
    [Tooltip("Minimum count of objects a pool can have at which point it must expand and instantiate new objects.")]
    [Range(0, 100)]
    [SerializeField]
    protected int expandThreshold;
    /// <summary>
    /// Count of objects that will be instantiated with each expand.
    /// </summary>
    [Tooltip("Count of objects that will be instantiated with each expand.")]
    [Range(1, 100)]
    [SerializeField]
    protected int expandCount;
    
    /// <summary>
    /// Pools of available objects. Not yet used in a scene.
    /// </summary>
    protected Dictionary<TPool, List<TPoolObject>> availablePools;
    /// <summary>
    /// Pools of unavailable objects. Objects used in a scene.
    /// </summary>
    protected Dictionary<TPool, HashSet<TPoolObject>> unavailablePools;

    [Tooltip("Maximum amount of time (in seconds) that expansion of a pool can take. " +
        "Used if instantiation is broken into multiple frames and/or expand count is too large for a single call.")]
    [Range(0.02f, 0.5f)]
    [SerializeField]
    protected float maxTimeForExpansion;
    protected bool isThresholdReached;
    protected HashSet<TPool> poolsToExpand;

    protected Transform myTransform;

    /// <summary>
    /// Get an object from the specified pool with pre/post-activation data, and set its parent.
    /// </summary>
    /// <param name="pool">Pool to get an object from.</param>
    /// <param name="parent">New parent of the pooled object.</param>
    /// <param name="preActivationData">Data used in the pre-activation of the pooled object.</param>
    /// <param name="postActivationData">Data used in the post-activation of the pooled object.</param>
    /// <returns></returns>
    public TPoolObject GetObject(TPool pool, Transform parent, System.Object preActivationData, System.Object postActivationData)
    {
        int countOfAvailableObjects = availablePools[pool].Count;

        // Expand if threshold reached or if the pool is empty
        if (countOfAvailableObjects == expandThreshold || countOfAvailableObjects == 0)
        {
            InstantiateObject(pool);
            countOfAvailableObjects = 1;

            isThresholdReached = true;
            poolsToExpand.Add(pool);
        }

        // Get the last object in the available and move it to the unavailable pool
        TPoolObject pooledObject = availablePools[pool][countOfAvailableObjects - 1];
        availablePools[pool].RemoveAt(countOfAvailableObjects - 1);
        unavailablePools[pool].Add(pooledObject);

        // Change parent and activate the object 
        pooledObject.transform.SetParent(parent);
        ActivatePoolObject(pooledObject, preActivationData, postActivationData);

        return pooledObject;
    }

    /// <summary>
    /// Get an object from the specified pool and set its parent.
    /// </summary>
    public TPoolObject GetObject(TPool pool, Transform parent)
    {
        TPoolObject pooledObject = GetObject(pool, parent, null, null);
        return pooledObject;
    }
        
    /// <summary>
    /// Return all used objects to their respective pools.
    /// </summary>
    public void ReclaimAllObjects()
    {
        // Go through all pools
        foreach (var poolObjectPair in unavailablePools)
        {
            // Go through all pool objects in the current pool and let the available pool claim them 
            // without deleting them from the unavailable pool
            foreach (var poolObject in poolObjectPair.Value)
            {
                ReclaimObject(poolObjectPair.Key, poolObject, false);
            }

            // Clear the current unavailable pool
            poolObjectPair.Value.Clear();
        }
    }

    /// <summary>
    /// Return the specified object to the pool.
    /// </summary>
    /// <param name="pool">Pool to which to return the object.</param>
    public void ReclaimObject(TPool pool, TPoolObject objectToReclaim)
    {
        ReclaimObject(pool, objectToReclaim, true);
    }

    protected virtual void Awake()
    {
        allPools = new Dictionary<TPool, TPoolObject>();
        availablePools = new Dictionary<TPool, List<TPoolObject>>();
        unavailablePools = new Dictionary<TPool, HashSet<TPoolObject>>();
        poolsToExpand = new HashSet<TPool>();

        myTransform = transform;      
    }

    protected virtual void Start()
    {
        foreach (var item in allPools)
        {
            availablePools.Add(item.Key, new List<TPoolObject>());
            unavailablePools.Add(item.Key, new HashSet<TPoolObject>());
        }

        InitializeDefaultActivePools();
    }
    
    protected virtual void Update()
    {        
        if (isThresholdReached)
        {
            isThresholdReached = false;
            foreach (TPool pool in poolsToExpand)
            {
                ExpandPool(pool, expandCount);
            }
            poolsToExpand.Clear();
        }
    }

    protected abstract void SetAllPools();
    protected abstract void InitializeDefaultActivePools();
    

    /// <summary>
    /// Instantiates an object for the specified pool and deactivates it.
    /// </summary>
    protected virtual void InstantiateObject(TPool pool)
    {
        TPoolObject instantiatedObject = Instantiate(allPools[pool], myTransform);
        availablePools[pool].Add(instantiatedObject);

        DeactivatePoolObject(instantiatedObject);
    }

    /// <summary>
    /// Expands the specified pool by instantiating new objects for it.
    /// </summary>
    /// <param name="newObjectCount">Count of objects to instantiate.</param>
    protected virtual void ExpandPool(TPool poolToExpand, int newObjectCount)
    {
        for (int i = 0; i < newObjectCount; i++)
        {
            InstantiateObject(poolToExpand);
        }
    }

    /// <summary>
    /// Removes the object from its unavailable pool (if necessary), adds it to the available pool and deactivates it.
    /// </summary>
    protected virtual void ReclaimObject(TPool pool, TPoolObject objectToReclaim, bool shouldRemoveImmediately)
    {
        if (shouldRemoveImmediately)
        {
            unavailablePools[pool].Remove(objectToReclaim);
        }

        availablePools[pool].Add(objectToReclaim);

        DeactivatePoolObject(objectToReclaim);
        objectToReclaim.transform.SetParent(myTransform, false);
    }

    /// <summary>
    /// Calls the specified pool object's pre/post-activation methods before/after activating the game object it belongs to.
    /// </summary>
    private void ActivatePoolObject(TPoolObject objectToActivate, System.Object preActivationData, System.Object postActivationData)
    {
        objectToActivate.PreActivation(preActivationData);
        objectToActivate.gameObject.SetActive(true);
        objectToActivate.PostActivation(postActivationData);
    }

    /// <summary>
    /// Calls the specified pool object's pre/post-deactivation methods before/after deactivating the game object it belongs to.
    /// </summary>
    private void DeactivatePoolObject(TPoolObject objectToDeactivate)
    {
        objectToDeactivate.PreDeactivation();
        objectToDeactivate.gameObject.SetActive(false);
        objectToDeactivate.PostDeactivation();
    }
}


