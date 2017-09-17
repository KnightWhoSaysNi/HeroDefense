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

    public Renderer[] renderers;                    // Renderes whose materials can change for valid/invalid placement and for placed object
    public Material[] transparentMaterials;         // Materials used by renderers to show valid placement    
    private Material[] originalMaterials;           // Original materials that will be used by renderers when the object is placed
    private Material[] illegalPlacementMaterials;
    private Color illegalPlacementColor;
    private int countOfRenderers;
    private bool isInIllegalState;

    protected bool isPlaced;
    private HashSet<Collider> currentlyCollided; // TODO If collided objects can dissapear they need to let this class know so it can remove them from the hash set (if they're in it)
    private int numberOfCollisions;

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
            return numberOfCollisions == 0;
        }
    }

    protected virtual int NumberOfCollisions
    {
        get
        {
            return numberOfCollisions;
        }
        set
        {
            numberOfCollisions = value;

            if (numberOfCollisions > 0)
            {
                // If already in illegal state no need to go through this loop again
                if (!isInIllegalState)
                {
                    isInIllegalState = true;
                    for (int i = 0; i < countOfRenderers; i++)
                    {
                        renderers[i].material = illegalPlacementMaterials[i];
                    }
                }
            }
            else
            {
                // Going back from illegal state to legal placement state (no collided placeables)
                isInIllegalState = false;
                for (int i = 0; i < countOfRenderers; i++)
                {
                    renderers[i].material = transparentMaterials[i];
                }
            }
        }
    }

    public void ResetCollisions()
    {
        NumberOfCollisions = 0;
        currentlyCollided.Clear();
    }

    protected virtual void Start()
    {
        currentlyCollided = new HashSet<Collider>();

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
            NumberOfCollisions++;
        }
        else
        {
            if (other.CompareTag("AttackArea")) // ADD TO CONST
            {
                // Collider of a child object of this or the other placeable so it's ignored
                return;
            }

            // Collider belongs to an object other than the placeable (or its children)
            currentlyCollided.Add(other);

            if (currentlyCollided.Count > 0)
            {
                NumberOfCollisions++;
            }
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
            NumberOfCollisions--;
        }        
        else 
        {
            if (other.CompareTag("AttackArea")) // ADD TO CONST
            {
                // Collider of a child object of this or the other placeable so it's ignored
                return;
            }
            
            // Collider belongs to an object other than the placeable (or its children)
            if (currentlyCollided.Count > 0)
            {
                NumberOfCollisions--;
            }
            currentlyCollided.Remove(other);
        }
    } 
}
