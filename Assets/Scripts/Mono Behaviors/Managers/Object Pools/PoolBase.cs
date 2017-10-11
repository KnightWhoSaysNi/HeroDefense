using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PoolBase<TPool, TPoolObject> : MonoBehaviour    
    where TPool : ScriptableObject 
    where TPoolObject : MonoBehaviour, IPoolable
{
    #region - Fields -
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
    protected Dictionary<TPool, Queue<TPoolObject>> availablePools;
    /// <summary>
    /// Pools of unavailable objects. Objects used in a scene.
    /// </summary>
    protected Dictionary<TPool, HashSet<TPoolObject>> unavailablePools;
    
    protected bool isExpandNeeded;
    protected HashSet<TPool> poolsToExpand;

    protected Transform myTransform;
    #endregion

    #region - Public methods -
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

        if (countOfAvailableObjects == 0)
        {
            // Since the pool is empty instantiate a new object on the fly - the object that will be returned
            InstantiateObject(pool);

            // If the expand count is 1, the above code is all that is required - 1 new object is instantiated as it becomes needed
            // Otherwise report that the pool needs to expand
            if (expandCount > 1)
            {
                isExpandNeeded = true;
                poolsToExpand.Add(pool);
            }
        }
        else if (countOfAvailableObjects == expandThreshold)
        {
            // Currently there is an available object to return, but report that the pool needs to expand since threshold is reached
            isExpandNeeded = true;
            poolsToExpand.Add(pool);
        }

        // Get an object from the available and move it to the unavailable pool
        TPoolObject pooledObject = availablePools[pool].Dequeue();
        unavailablePools[pool].Add(pooledObject);

        // Change parent and activate the object 
        pooledObject.transform.SetParent(parent);
        ActivatePoolObject(pooledObject, preActivationData, postActivationData);

        return pooledObject;
    }

    /// <summary>
    /// Get an object from the specified pool and set its parent.
    /// </summary>
    /// <param name="pool">Pool to get an object from.</param>
    /// <param name="parent">New parent of the pooled object.</param>
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
            ReclaimAllObjectFromPool(poolObjectPair.Key);
        }
    }

    /// <summary>
    /// Return all used objects of the specified pool.
    /// </summary>
    public void ReclaimAllObjectFromPool(TPool pool)
    {
        // Go through all pool objects in the current pool and let the available pool claim them 
        // without deleting them from the unavailable pool
        foreach (var poolObject in unavailablePools[pool])
        {
            ReclaimObject(pool, poolObject, false);
        }

        // Clear the unavailable pool
        unavailablePools[pool].Clear();
    }

    /// <summary>
    /// Destroy all game objects of the specified pool. Game objects are first returned to the available pool.
    /// </summary>
    public void ClearPool(TPool poolToClear)
    {
        ReclaimAllObjectFromPool(poolToClear);

        while (availablePools[poolToClear].Count > 0)
        {
            TPoolObject poolObject = availablePools[poolToClear].Dequeue();
            Destroy(poolObject);
        }
    }
    #endregion

    #region - Abstract methods -
    public abstract void ReclaimObject(TPoolObject objectToReclaim);

    protected abstract void SetAllPools();
    protected abstract void InitializeDefaultActivePools();
    #endregion

    #region - MonoBehavior methods -
    protected virtual void Awake()
    {
        allPools = new Dictionary<TPool, TPoolObject>();
        availablePools = new Dictionary<TPool, Queue<TPoolObject>>();
        unavailablePools = new Dictionary<TPool, HashSet<TPoolObject>>();
        poolsToExpand = new HashSet<TPool>();

        myTransform = transform;
    }

    protected virtual void Start()
    {
        foreach (var item in allPools)
        {
            availablePools.Add(item.Key, new Queue<TPoolObject>());
            unavailablePools.Add(item.Key, new HashSet<TPoolObject>());
        }

        InitializeDefaultActivePools();
    }

    protected virtual void Update()
    {
        if (isExpandNeeded)
        {
            isExpandNeeded = false;
            foreach (TPool pool in poolsToExpand)
            {
                ExpandPool(pool, expandCount);
            }
            poolsToExpand.Clear();
        }
    }
    #endregion

    #region - Protected methods -
    /// <summary>
    /// Instantiates an object for the specified pool and deactivates it.
    /// </summary>
    protected virtual void InstantiateObject(TPool pool)
    {
        TPoolObject instantiatedObject = Instantiate(allPools[pool], myTransform);
        availablePools[pool].Enqueue(instantiatedObject);

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

        availablePools[pool].Enqueue(objectToReclaim);

        DeactivatePoolObject(objectToReclaim);
        objectToReclaim.transform.SetParent(myTransform, false);
    }
    #endregion

    #region - Private methods -
    /// <summary>
    /// Calls the specified pool object's pre/post-activation methods before/after activating the game object it belongs to.
    /// </summary>
    private void ActivatePoolObject(TPoolObject objectToActivate, System.Object preActivationData, System.Object postActivationData)
    {
        objectToActivate.DoPreActivation(preActivationData);
        objectToActivate.gameObject.SetActive(true);
        objectToActivate.DoPostActivation(postActivationData);
    }

    /// <summary>
    /// Calls the specified pool object's pre/post-deactivation methods before/after deactivating the game object it belongs to.
    /// </summary>
    private void DeactivatePoolObject(TPoolObject objectToDeactivate)
    {
        objectToDeactivate.DoPreDeactivation();
        objectToDeactivate.gameObject.SetActive(false);
        objectToDeactivate.DoPostDeactivation();
    } 
    #endregion
}