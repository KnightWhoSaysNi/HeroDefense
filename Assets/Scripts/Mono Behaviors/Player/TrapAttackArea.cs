using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrapAttackArea : MonoBehaviour
{
    public LayerMask enemyLayerMask; // TODO perhaps add to const?

    private Dictionary<Collider, Enemy> enemiesInArea;

    public delegate void EnemyTrackHandler(Enemy enemy, bool isInAttackArea);
    public event EnemyTrackHandler EnemyMovementRegistered;

    private void Awake()
    {
        enemiesInArea = new Dictionary<Collider, Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) // ADD TO CONST
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                EnemyMovementRegistered?.Invoke(enemy, true);
            }

            enemiesInArea.Add(other, enemy);           
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy")) // ADD TO CONST
        {
            // This should always be true, but the check is O(1)
            if (enemiesInArea.ContainsKey(other)) 
            {
                Enemy enemy = enemiesInArea[other];
                enemiesInArea.Remove(other);
                EnemyMovementRegistered?.Invoke(enemy, false);
            }            
        }
    }
}
