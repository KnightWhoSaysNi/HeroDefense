using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Placeable : MonoBehaviour // TODO Write custom editor for this class
{    
    [HideInInspector]
    public Vector3 position;
    /// <summary>
    /// Value by which to multiple a normalized vector that is added to the placeable's local up position in order to raise/lower it. 
    /// If left at 0 placeable won't be raised at all. Blender files should already be set correctly and don't need this value.
    /// </summary>
    [Tooltip("Value by which to multiple a normalized vector that is added to the placeable's local up position in order to raise/lower it." +
        " If left at 0 placeable won't be raised at all. Blender files should already be set correctly and don't need this value.")]
    public float placementOffsetMutiplier;    

    public Renderer[] renderers;                    // Renderes whose materials can change for valid/invalid placement and for placed object
    public Material[] transparentMaterials;         // Materials used by renderers to show valid placement    
    private Material[] originalMaterials;           // Original materials that will be used by renderers when the object is placed
    private Material[] illegalPlacementMaterials;
    private Color illegalPlacementColor;
    private int countOfRenderers;
    private bool isInIllegalState;

    protected bool isPlaced;
    // TODO If collided objects can dissapear they need to let this class know so it can remove them from the hash set (if they're in it)
    // The placeable needs to have at least 1 collided object - the one one which it is being placed
    public HashSet<GameObject> currentlyCollidedObjects; 
    private int numberOfCollidedPlaceables;

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
                position = transform.position; // TODO perhaps add collider's bounds extents

                // Placed placeables use their original materials
                for (int i = 0; i < countOfRenderers; i++)
                {
                    renderers[i].material = originalMaterials[i];
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

    public void ResetCollisions()
    {
        NumberOfCollidedPlaceables = 0;
        currentlyCollidedObjects.Clear();
    }

    protected virtual void Start()
    {
        currentlyCollidedObjects = new HashSet<GameObject>();

        countOfRenderers = renderers.Length;
        originalMaterials = new Material[countOfRenderers];
        illegalPlacementMaterials = new Material[countOfRenderers];
        illegalPlacementColor = new Color(1, 0, 0, 0.75f);

        for (int i = 0; i < countOfRenderers; i++)
        {
            // Cache original materials - the materials used in placed state
            originalMaterials[i] = renderers[i].sharedMaterial;

            // Create and cache illegal placement materials based on the original ones
            Material illegalPlacementMaterial = new Material(transparentMaterials[i]);
            illegalPlacementMaterial.color = illegalPlacementColor;
            illegalPlacementMaterials[i] = illegalPlacementMaterial;

            // At the start this object is not yet placed so transparent materials are used
            renderers[i].material = transparentMaterials[i];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlaced)
        {
            // Object is placed and doesn't care about collisions
            return;
        }

        if (other.CompareTag("Placeable")) // ADD TO CONST
        {
            NumberOfCollidedPlaceables++;
        }
        else
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

        if (other.CompareTag("Placeable")) 
        {
            NumberOfCollidedPlaceables--;
        }        
        else 
        {
            currentlyCollidedObjects.Remove(other.gameObject);
            CheckPlacementValidity();
        }
    } 

    private void CheckPlacementValidity()
    {
        if (numberOfCollidedPlaceables == 0 && currentlyCollidedObjects.Count == 1)
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
                    renderers[i].material = illegalPlacementMaterials[i];
                }
            }
        }
    }
}
