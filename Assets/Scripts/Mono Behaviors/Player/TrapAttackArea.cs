using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrapAttackArea : MonoBehaviour
{
    public LayerMask enemyLayerMask; // TODO perhaps add to const?

    public delegate void EnemyTrackHandler(Enemy enemy, bool isInAttackArea);
    public event EnemyTrackHandler EnemyMovementRegistered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) // ADD TO CONST
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                EnemyMovementRegistered?.Invoke(enemy, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy")) // ADD TO CONST
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                EnemyMovementRegistered?.Invoke(enemy, false);
            }
        }
    }
}
