using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData : InspectorData
{
    public Transform playerStartBearings;
    public Transform endPoint;
    public Transform placeableParent;
    public Transform enemyParent;
}
