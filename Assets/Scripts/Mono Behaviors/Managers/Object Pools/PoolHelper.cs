using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Holds Activate/Deactivate methods for each pool object.
/// </summary>
public static class PoolHelper
{
    // TODO Refactor by creating different activations/deactivations for all the varieties of placeables and enemies that require different activations/deactivations.
    // Spikes trap and FirePillar might activate/deactivate in a different way, even though they're both Traps and will each call the same Trap.OnSold method
    private static Placeable placeableObject; 
    private static Enemy enemyObject;
    private static bool shouldActivate;

    public static void ActivateObject<T>(T objectToActivate) where T : MonoBehaviour
    {
        shouldActivate = true;
        ChangeObject(objectToActivate);
    }

    public static void DeactivateObject<T>(T objectToDeactivate) where T : MonoBehaviour
    {
        shouldActivate = false;
        ChangeObject(objectToDeactivate);
    }

    private static void ChangeObject<T>(T objectToChange) where T : MonoBehaviour
    {   
        if (objectToChange is Placeable)
        {
            placeableObject = objectToChange as Placeable;
            ChangePlaceable();
        }
        else if (objectToChange is Enemy)
        {
            enemyObject = objectToChange as Enemy;
            ChangeEnemy();
        }                
    }

    
    private static void ChangePlaceable()
    {
        // Set placeable specific properties to the value of isActivationChange

        if (placeableObject as Trap)
        {
            // Set trap specific properties to the value of isActivationChange (something different for each trap that cannot be done in the Trap.OnSold)
        }
    }

    private static void ChangeEnemy()
    {
        // Set Enemy specific properties to the value of isActivationChange (something different for each enemy that cannot be done in the Enemy class itself)                
    }

}
