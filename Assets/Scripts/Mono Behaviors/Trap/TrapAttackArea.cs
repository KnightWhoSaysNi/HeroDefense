using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class TrapAttackArea : MonoBehaviour
{
    public LayerMask enemyLayerMask; // ADD TO CONST
    public Dictionary<Collider, Enemy> enemiesInArea;

    public delegate void EnemyTrackHandler(Enemy enemy, bool isInAttackArea);
    public event EnemyTrackHandler EnemyMovementRegistered;

    private void Awake()
    {
        enemiesInArea = new Dictionary<Collider, Enemy>();
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void Start()
    {
        Enemy.EnemyDied += OnEnemyDied;
    }

    /// <summary>
    /// If the enemy that has died was in enemiesInArea dictionary this removes that enemy by its provided collider key.
    /// </summary>
    private void OnEnemyDied(Enemy enemy, Collider enemyCollider)
    {
        if (enemiesInArea.ContainsKey(enemyCollider))
        {
            enemiesInArea.Remove(enemyCollider);
        }
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
