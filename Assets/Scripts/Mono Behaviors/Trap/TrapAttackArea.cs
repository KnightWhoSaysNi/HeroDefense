using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class TrapAttackArea : MonoBehaviour
{
    public LayerMask enemyLayerMask;
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
    /// If the enemy that has died was in <see cref="enemiesInArea"/> dictionary this removes that enemy by its provided collider key.
    /// This is done so the dictionary doesn't grow too big if the level has a large number of enemies.
    /// </summary>
    private void OnEnemyDied(Enemy enemy, Collider enemyCollider, bool hasFinishedLevel)
    {
        if (enemiesInArea.ContainsKey(enemyCollider))
        {
            enemiesInArea.Remove(enemyCollider);
            EnemyMovementRegistered?.Invoke(enemy, false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.EnemyTag)) 
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            // TODO The second check shouldn't really be necessary (dead enemies shouldn't move any more). Unless the trap itself moves
            if (enemy != null && !enemy.IsDead)  
            {
                enemiesInArea.Add(other, enemy);           
                EnemyMovementRegistered?.Invoke(enemy, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.EnemyTag)) 
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
