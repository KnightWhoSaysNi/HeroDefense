using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManagerHelper : MonoBehaviour // TODO Create custom editor
{
    public Level level;
    [Space(10)]
    public Transform playerStartBearings;
    public Transform spawnPoint;
    public Transform endPoint;
    public Transform enemyParent;

    private void Awake()
    {
        if (level == null || playerStartBearings == null || spawnPoint == null || endPoint == null || enemyParent == null)
        {
            throw new UnityException($"{typeof(LevelManagerHelper)} class doesn't have all of its fields set up. {typeof(LevelManager)} cannot initiate a level.");
        }

        if (spawnPoint == endPoint)
        {
            throw new UnityException("Spawn point and end point cannot be the same.");
        }

        LevelManager.SetUpLevel(this);
    }
}
