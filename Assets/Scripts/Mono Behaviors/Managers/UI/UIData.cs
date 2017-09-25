using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for classes holding UI elements for different UI helper classes that talk to the UIManager.
/// </summary>
public abstract class UIData
{
    public bool AreAllElementsSet()
    {
        System.Type callerType = this.GetType();
        var publicFields = callerType.GetFields();

        for (int i = 0; i < publicFields.Length; i++)
        {
            if (publicFields[i].GetValue(this) == null)
            {
                return false;
            }
        }

        return true;
    }
}
