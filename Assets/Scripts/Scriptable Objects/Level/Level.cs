using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    /// <summary>
    /// A list of waves along with additional level information, like the wave count.
    /// </summary>
    public List<LevelElement> levelElements;
}
