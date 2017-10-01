using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for helper classes holding objects set in the inspector.
/// </summary>
public abstract class InspectorData
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
