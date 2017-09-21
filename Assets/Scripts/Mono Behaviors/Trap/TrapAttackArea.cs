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
    }

    private void Start()
    {
        Enemy.EnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        enemiesInArea.Clear();
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
            // TODO The second check shouldn't really be necessary (dead enemies shouldn't move any more). Unless the trap itself moves
            if (enemy != null && !enemy.isDead)  
            {
                enemiesInArea.Add(other, enemy);           
                EnemyMovementRegistered?.Invoke(enemy, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy")) // ADD TO CONST
        {
            if (enemiesInArea.ContainsKey(other))
            {
                Enemy enemy = enemiesInArea[other];
                enemiesInArea.Remove(other);
                EnemyMovementRegistered?.Invoke(enemy, false);
            }
        }
    }
}
