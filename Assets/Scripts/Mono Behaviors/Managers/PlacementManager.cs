using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlacementManager : Raycaster
{
    #region - Fields -
    [Range(0, 1)]
    [SerializeField]
    private float percentReturnOnSell;
    public List<Placeable> placeables;
    [HideInInspector]
    public Transform placeableParent;
    private Placeable activePlaceable;
    private int placeableIndex;
    private bool isIndexChanged;

    private bool isInPlacementMode;
    private bool canBePlaced;
    [Tooltip("Max distance at which placement of the placeable can happen.")]
    [SerializeField]
    private float placementRange = 100;
    [Tooltip("How much to rotate the placeable by, in degrees, with each scroll action.")]
    [SerializeField]
    private float rotationDegreesAmount;
    private float currentRotationDegrees;
    private float scrollValue;

    /// <summary>
    /// Position of the placeable while it has not yet been placed. Same as the actual placement position, but with some offset.
    /// </summary>
    private Vector3 visualizationPlacementPosition;
    private Vector3 visualizationOffset;
    private Vector3 zFightingOffset;
    private Vector3 placementPosition;
    private Vector3 lastPlacedPosition;
    private Vector3 zeroVector;
    #endregion

    #region - Events -
    public static event Action<bool> PlacementModeChanged; 
    #endregion

    #region - "Singleton" Instance -
    private static PlacementManager instance;

    public static PlacementManager Instance
    {
        get
        {
            if (instance == null)
            {
                throw new UnityException("Someone is calling PlacementManager.Instance before it is set! Change script execution order.");
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

    #region - Properties -
    private bool CanBePlaced
    {
        get
        {
            canBePlaced =
                Input.GetButtonDown("Fire1") &&
                Player.Instance.Gold >= activePlaceable.GoldCost &&
                placementPosition != lastPlacedPosition &&
                activePlaceable.CanBePlaced;

            return canBePlaced;
        }
    }
    #endregion

    #region - Public methods -
    /// <summary>
    /// Calculates and returns the sell price for the placeable.
    /// </summary>
    public int GetSellPrice(Placeable placeableToSell)
    {
        return (int)(placeableToSell.GoldCost * percentReturnOnSell);
    }

    /// <summary>
    /// Sells the placeable and returns some gold to the player.
    /// </summary>
    public void SellPlaceable(Placeable placeableToSell)
    {
        placeableToSell.Sell();
        Player.Instance.Gold += GetSellPrice(placeableToSell);

        // Return to the pool
        PlaceablePool.Instance.ReclaimObject(placeableToSell.placeableType, placeableToSell);
    }
    #endregion

    #region - MonoBehavior methods -
    protected new void Awake()
    {
        InitializeSingleton();

        base.Awake();

        zeroVector = new Vector3(0, 0, 0);
        zFightingOffset = new Vector3(0.001f, 0.001f, 0.001f); 
    }

    private void Start()
    {
        SceneManager.sceneLoaded += (loadedScene, loadSceneMode) => ResetPlacement();
        LevelManager.LevelRestarted += ResetPlacement;
    }

    private void Update()
    {
        CheckForActivePlaceableChange();
        CheckForPlacementModeToggle();
        CheckForPlacement();
    }
    #endregion

    #region - Private methods -
    /// <summary>
    /// Checks if the user has requested to build a different placeable than the one currently active.
    /// </summary>
    private void CheckForActivePlaceableChange()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && placeableIndex != 0)
        {
            placeableIndex = 0;
            isIndexChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && placeableIndex != 1)
        {
            placeableIndex = 1;
            isIndexChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && placeableIndex != 2)
        {
            placeableIndex = 2;
            isIndexChanged = true;
        }
        // TODO Refactor the above part when more traps are created and more slots are used

        if (isIndexChanged)
        {
            UIManager.Instance.ChangeActiveSlot(placeableIndex);

            if (activePlaceable != null)
            {
                PlaceablePool.Instance.ReclaimObject(activePlaceable.placeableType, activePlaceable);
                activePlaceable = null;
            }

            isIndexChanged = false;
        }
    }

    /// <summary>
    /// Changes placement mode if the placement input key has been pressed.
    /// </summary>
    private void CheckForPlacementModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isInPlacementMode = !isInPlacementMode;
            PlacementModeChanged?.Invoke(isInPlacementMode);

            if (activePlaceable != null)
            {
                activePlaceable.gameObject.SetActive(isInPlacementMode);
            }
        }
    }

    #region - Check for placement -
    /// <summary>
    /// Checks if in placement mode. Checks if camera is pointing at a place where the placeable can be placed. 
    /// Checks all other conditions for placement. If every check passes calls for placing the active placeable.
    /// </summary>
    private void CheckForPlacement()
    {
        if (isInPlacementMode)
        {
            // A check to see if the active placeable is already placed. If it is not, use that instance, and if it is already placed then get a new one from the pool
            if (activePlaceable == null || activePlaceable.IsPlaced)
            {
                activePlaceable = PlaceablePool.Instance.GetObject(placeables[placeableIndex].placeableType, placeableParent);
            }

            // Active placeable is not null at this point and is on an instantiated game object
            cameraRay = playerCamera.ViewportPointToRay(viewportCenter);

            if (Physics.Raycast(cameraRay, out raycastHit, placementRange, raycastHitLayerMask))
            {
                // Ray hit an area that the placeable can be placed on
                activePlaceable.gameObject.SetActive(true);

                FindPlacementPositions();
                UpdateActivePlaceableTransform();
                CheckForRotation();

                if (CanBePlaced)
                {
                    PlacePlaceable();
                }
            }
            else
            {
                // Ray hit an area that the placeable cannot be placed on
                activePlaceable.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Finds <see cref="visualizationOffset"/>, <see cref="visualizationPlacementPosition"/> and <see cref="placementPosition"/>.
    /// </summary>
    private void FindPlacementPositions()
    {
        // In case the placeable's pivot point is not correctly set, this offset raises/lowers it to a correct position for placement
        visualizationOffset = raycastHit.normal * activePlaceable.PlacementOffsetMultiplier + zFightingOffset;

        visualizationPlacementPosition = raycastHit.point + visualizationOffset;
        placementPosition = visualizationPlacementPosition - zFightingOffset;
    }

    /// <summary>    
    /// Sets the active placeable's rotation and sets its position to the <see cref="visualizationPlacementPosition"/>.
    /// </summary>
    private void UpdateActivePlaceableTransform()
    {
        activePlaceable.transform.position = visualizationPlacementPosition;
        activePlaceable.transform.rotation = Quaternion.FromToRotation(Vector3.up, raycastHit.normal);
    }

    /// <summary>
    /// Checks if scroll wheel was used and rotates the active placeable by the apropriate amount.
    /// </summary>
    private void CheckForRotation()
    {
        scrollValue = Input.GetAxisRaw("Mouse ScrollWheel");

        if (scrollValue != 0)
        {
            scrollValue = scrollValue > 0 ? 1 : -1; // GetAxisRaw for scroll wheel doesn't return whole numbers

            // Rotates the placeable by rotationDegreesAmount 
            currentRotationDegrees += scrollValue * rotationDegreesAmount;

            if (currentRotationDegrees == 360 || currentRotationDegrees == -360)
            {
                currentRotationDegrees = 0;
            }
        }

        activePlaceable.transform.Rotate(raycastHit.normal, currentRotationDegrees, Space.World);
    }

    /// <summary>
    /// Places the placeable at the <see cref="placementPosition"/>, updates player's current gold and also the <see cref="lastPlacedPosition"/>.
    /// </summary>
    private void PlacePlaceable()
    {
        Player.Instance.Gold -= activePlaceable.GoldCost;

        activePlaceable.IsPlaced = true;
        activePlaceable.transform.position = placementPosition;
        activePlaceable = null;

        lastPlacedPosition = placementPosition;
        //currentRotationDegrees = 0;
    }
    #endregion

    private void ResetPlacement()
    {
        isInPlacementMode = false;
        placeableIndex = 0;
        lastPlacedPosition = zeroVector;

        if (activePlaceable != null)
        {
            PlaceablePool.Instance.ReclaimObject(activePlaceable.placeableType, activePlaceable);
            activePlaceable = null;
        }
    } 
    #endregion
}
