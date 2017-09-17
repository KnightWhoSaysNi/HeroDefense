using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Placeable placeable; // TEST

    private List<Placeable> availablePlaceables;
    private Placeable activePlaceable;

    [SerializeField] private LayerMask placementLayerMask;
    private Camera mainCamera;
    private Vector3 viewportCenter;
    private Ray cameraRay;
    private RaycastHit placementHit;

    [HideInInspector]
    public bool isInPlacementMode;
    private Vector3 lastPlacedPosition;
    private Vector3 visualizationOffset; 
    private Vector3 zFightingOffset;
    private float rotationAngleDegrees;

    void Start()
    {
        viewportCenter = new Vector3(0.5f, 0.5f, 0);
        zFightingOffset = new Vector3(0.01f, 0.01f, 0.01f); // ADD TO CONST maybe?

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {            
            isInPlacementMode = !isInPlacementMode;

            if (activePlaceable != null)
            {
                activePlaceable.gameObject.SetActive(isInPlacementMode);

                if (!isInPlacementMode)
                {
                    activePlaceable.ResetCollisions();
                }
            }
        }

        if (isInPlacementMode)
        {
            // A check to see if the current placeable is already placed. If it is not use that instance, and if it is already placed then instantiate a new placeable
            if (activePlaceable == null || activePlaceable.IsPlaced)
            {
                activePlaceable = GameObject.Instantiate(placeable.gameObject).GetComponent<Placeable>(); // TEST // TODO Create an object pool
            }
            
            cameraRay = mainCamera.ViewportPointToRay(viewportCenter);

            if (Physics.Raycast(cameraRay, out placementHit, 100, placementLayerMask))
            {
                activePlaceable.gameObject.SetActive(true);

                // In case the placeable's pivot point is not correctly set this offset raises/lowers it to a correct position for placement
                visualizationOffset = placementHit.normal * activePlaceable.placementOffsetMutiplier + zFightingOffset;

                Vector3 visualizationPlacementPosition = placementHit.point + visualizationOffset;
                Vector3 placementPosition = visualizationPlacementPosition - zFightingOffset;

                activePlaceable.transform.position = visualizationPlacementPosition;                
                activePlaceable.transform.rotation = Quaternion.FromToRotation(Vector3.up, placementHit.normal); 

                float scrollValue = Input.GetAxisRaw("Mouse ScrollWheel");                
                if (scrollValue != 0)
                {
                    scrollValue = scrollValue > 0 ? 1 : -1; // GetAxisRaw for scroll wheel doesn't return whole numbers

                    // Rotates the placeable by 5 degrees
                    rotationAngleDegrees += scrollValue * 5; // TEST store the right values somewhere
                    if (rotationAngleDegrees == 360 || rotationAngleDegrees == -360)
                    {
                        rotationAngleDegrees = 0;
                    }
                }
                activePlaceable.transform.Rotate(placementHit.normal, rotationAngleDegrees, Space.World);
                                
                if (Input.GetButtonDown("Fire1") && activePlaceable.CanBePlaced && placementPosition != lastPlacedPosition)
                {
                    activePlaceable.IsPlaced = true;
                    activePlaceable.transform.position = placementPosition;
                    activePlaceable = null;

                    lastPlacedPosition = placementPosition;
                }
            }
            else
            {
                activePlaceable.gameObject.SetActive(false);
                activePlaceable.ResetCollisions();
            }
        }
    }
}
