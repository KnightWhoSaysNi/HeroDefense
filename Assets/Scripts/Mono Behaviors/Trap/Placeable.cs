﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Placeable : MonoBehaviour // TODO Write custom editor | Refactor
{
    public PlaceableType placeableType;
    public GameObject rangeVisual;

    [SerializeField]
    protected int goldCost;
    [HideInInspector]
    protected Vector3 position;
    /// <summary>
    /// Value by which to multiple a normalized vector that is added to the placeable's local up position in order to raise/lower it. 
    /// If left at 0 placeable won't be raised at all. Blender files should already be set correctly and don't need this value.
    /// </summary>
    [Tooltip("Value by which to multiple a normalized vector that is added to the placeable's local up position in order to raise/lower it." +
        " If left at 0 placeable won't be raised at all. Blender files should already be set correctly and don't need this value.")]
    [SerializeField] private float placementOffsetMultiplier;    

    [SerializeField] private Renderer[] renderers;                    // Renderes whose materials can change for valid/invalid placement and for placed object
    [SerializeField] private Material[] transparentMaterials;         // Materials used by renderers to show valid placement    
    [SerializeField] private Material illegalPlacementMaterial; // TODO Replace the illegalPlacementMaterials array with just 1 illegal placement material ADD TO CONST
    private Material[] originalMaterials;           // Original materials that will be used by renderers when the object is placed    

    private Color illegalPlacementColor;
    private int countOfRenderers;
    private bool isInIllegalState;

    protected bool isPlaced;
    // TODO If collided objects can dissapear they need to let this class know so it can remove them from the hash set (if they're in it)
    // The placeable needs to have at least 1 collided object - the one one which it is being placed
    public List<GameObject> currentlyCollidedObjects; // TEST Set this back to private HashSet
    private int numberOfCollidedPlaceables;

    public float PlacementOffsetMultiplier
    {
        get
        {
            return placementOffsetMultiplier;
        }
    }
    public int GoldCost
    {
        get
        {
            return goldCost;
        }
    }
    public bool IsPlaced
    {
        get
        {
            return isPlaced;
        }
        set
        {
            isPlaced = value;

            if (isPlaced)
            {
                // Whenever the placeable is placed its position is cached. 
                // If it is removed the position doesn't need to be updated as it is not supposed to be called in that case
                position = transform.position; 

                // Placed placeables use their original materials
                for (int i = 0; i < countOfRenderers; i++)
                {
                    renderers[i].material = originalMaterials[i];
                }

                OnPlaced();
            }
            else
            {
                for (int i = 0; i < countOfRenderers; i++)
                {
                    renderers[i].material = transparentMaterials[i];
                }
            }
        }
    }
    public virtual bool CanBePlaced
    {
        get
        {
            return numberOfCollidedPlaceables == 0 && currentlyCollidedObjects.Count == 1; // TODO perhaps add another check currentlyCollidedObjects[0].CompareTag
        }
    }
    protected virtual int NumberOfCollidedPlaceables
    {
        get
        {
            return numberOfCollidedPlaceables;
        }
        set
        {
            numberOfCollidedPlaceables = value;
            CheckPlacementValidity();
        }
    }

    /// <summary>
    /// Return the state of the placeable to before it was placed.
    /// </summary>
    public void Sell()
    {        
        OnSold();
        // OnDisable will be called after this method as the game object will be deactivated
    }

    protected void Awake()
    {
        currentlyCollidedObjects = new List<GameObject>();

        countOfRenderers = renderers.Length;
        originalMaterials = new Material[countOfRenderers];
        illegalPlacementColor = new Color(1, 0, 0, 0.75f); // ADD TO CONST

        for (int i = 0; i < countOfRenderers; i++)
        {
            // Cache original materials - the materials used in placed state
            originalMaterials[i] = renderers[i].sharedMaterial;

            // Create and cache illegal placement materials based on the original ones
            Material illegalPlacementMaterial = new Material(transparentMaterials[i]);
            illegalPlacementMaterial.color = illegalPlacementColor;

            // At the start this object is not yet placed so transparent materials are used
            renderers[i].material = transparentMaterials[i];
        }
    }

    protected void Start()
    {
        print("Refactor: Change public fields to serialized private fields wherever possible (which should be true for all scripts");
        if (isPlaced)
        {
            IsPlaced = true;
        }
    }

    protected virtual void OnDisable()
    {
        IsPlaced = false;
        isInIllegalState = false;
        numberOfCollidedPlaceables = 0;       
        currentlyCollidedObjects.Clear();
        rangeVisual.SetActive(true);
    }

    // Action to be overriden for each placeable that needs additional logic after it's been placed
    protected virtual void OnPlaced()
    {
        rangeVisual.SetActive(false);
    }

    // Action to be overriden for each placeable that needs additional logic after it's been sold
    protected virtual void OnSold()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlaced)
        {
            // Object is placed and doesn't care about collisions
            return;
        }

        if (other.CompareTag("Placeable") || other.CompareTag("Sellable")) // ADD TO CONST
        {
            NumberOfCollidedPlaceables++;
        }
        else if (!other.CompareTag("Enemy")) // ADD TO CONST
        {

            currentlyCollidedObjects.Add(other.gameObject);
            CheckPlacementValidity();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPlaced)
        {
            // Object is placed and doesn't care about collisions
            return;
        }

        if (other.CompareTag("Placeable") || other.CompareTag("Sellable")) 
        {
            NumberOfCollidedPlaceables--;
        }        
        else if (!other.CompareTag("Enemy")) // ADD TO CONST
        {
            currentlyCollidedObjects.Remove(other.gameObject);
            CheckPlacementValidity();
        }
    } 

    private void CheckPlacementValidity()
    {
        if (numberOfCollidedPlaceables == 0 && currentlyCollidedObjects.Count == 1 && Player.Instance.Gold >= goldCost)
        {
            // Placement is legal
            isInIllegalState = false;
            for (int i = 0; i < countOfRenderers; i++)
            {
                renderers[i].material = transparentMaterials[i];
            }
        }
        else
        {
            // Placement is illegal
            if (!isInIllegalState)
            {
                isInIllegalState = true;
                for (int i = 0; i < countOfRenderers; i++)
                {
                    renderers[i].material = illegalPlacementMaterial;
                }
            }
        }
    }
}
