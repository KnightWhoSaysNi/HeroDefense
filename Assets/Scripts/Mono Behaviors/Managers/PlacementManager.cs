using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : Raycaster
{
    public List<Placeable> placeables;
    [HideInInspector] public int placeableIndex;
    private Placeable activePlaceable;
    
    [HideInInspector]
    public bool isInPlacementMode;
    private Vector3 lastPlacedPosition;
    private Vector3 visualizationOffset;
    private Vector3 zFightingOffset;
    private float rotationAngleDegrees;

    protected new void Awake()
    {
        base.Awake();

        // Instead of making this class a "singleton" a simple check is made. This way only GameManager game object can have this mono behavior attached
        if (GetComponent<GameManager>() == null)
        {
            throw new UnityException($"{gameObject.name} game object has a {typeof(PlacementManager)} MonoBehavior attached. Only GameManager is allowed to have that script.");
        }

        zFightingOffset = new Vector3(0.001f, 0.001f, 0.001f);  // ADD TO CONST maybe?
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            placeableIndex = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            placeableIndex = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            placeableIndex = 2;
        }


        if (Input.GetKeyDown(KeyCode.B))
        {            
            isInPlacementMode = !isInPlacementMode;

            if (activePlaceable != null)
            {
                activePlaceable.gameObject.SetActive(isInPlacementMode);
            }
        }

        if (isInPlacementMode)
        {
            // A check to see if the active placeable is already placed. If it is not use that instance, and if it is already placed then instantiate a new placeable
            if (activePlaceable == null || activePlaceable.IsPlaced)
            {
                activePlaceable = GameObject.Instantiate(placeables[placeableIndex].gameObject).GetComponent<Placeable>(); // TEST // TODO Create an object pool
            }
            
            cameraRay = playerCamera.ViewportPointToRay(viewportCenter);

            if (Physics.Raycast(cameraRay, out raycastHit, 100f, raycastHitLayerMask))
            {
                activePlaceable.gameObject.SetActive(true);

                // In case the placeable's pivot point is not correctly set, this offset raises/lowers it to a correct position for placement
                visualizationOffset = raycastHit.normal * activePlaceable.placementOffsetMutiplier + zFightingOffset;

                Vector3 visualizationPlacementPosition = raycastHit.point + visualizationOffset;
                Vector3 placementPosition = visualizationPlacementPosition - zFightingOffset;

                activePlaceable.transform.position = visualizationPlacementPosition;                
                activePlaceable.transform.rotation = Quaternion.FromToRotation(Vector3.up, raycastHit.normal); 

                float scrollValue = Input.GetAxisRaw("Mouse ScrollWheel");                
                if (scrollValue != 0)
                {
                    scrollValue = scrollValue > 0 ? 1 : -1; // GetAxisRaw for scroll wheel doesn't return whole numbers

                    // Rotates the placeable by 5 degrees
                    rotationAngleDegrees += scrollValue * 5; // TODO store the right values somewhere
                    if (rotationAngleDegrees == 360 || rotationAngleDegrees == -360)
                    {
                        rotationAngleDegrees = 0;
                    }
                }
                activePlaceable.transform.Rotate(raycastHit.normal, rotationAngleDegrees, Space.World);
                                
                if (Input.GetButtonDown("Fire1") && activePlaceable.CanBePlaced && placementPosition != lastPlacedPosition)
                {
                    // TODO the placeable can be placed, but now check if the player has enough gold for the actual trap
                    activePlaceable.IsPlaced = true;
                    activePlaceable.transform.position = placementPosition;
                    activePlaceable = null;

                    lastPlacedPosition = placementPosition;
                }
            }
            else
            {
                activePlaceable.gameObject.SetActive(false);
            }
        }
    }
}
