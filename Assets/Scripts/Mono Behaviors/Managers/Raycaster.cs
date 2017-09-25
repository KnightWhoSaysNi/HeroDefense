using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycaster : MonoBehaviour
{
    public LayerMask raycastHitLayerMask;
    [HideInInspector]
    public Camera playerCamera;

    protected Vector3 viewportCenter;

    protected Ray cameraRay;
    protected RaycastHit raycastHit;

    protected void Awake()
    {
        viewportCenter = new Vector3(0.5f, 0.5f, 0);
    }
}
