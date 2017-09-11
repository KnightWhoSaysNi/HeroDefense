using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Trap : Placeable
{
    public TrapData trapData;
    private WaitForSeconds attackCooldown;

    public TrapAttackArea trapAttackArea;
    private List<Enemy> enemiesInRange;
    private Enemy currentEnemy;
    private IEnumerator attackCoroutine;

    public Material illegalPlacementMaterial;
    private Material originalMaterial;
    private new Renderer renderer;


    protected override void DisplayPlacementValidity(bool canBePlaced)
    {
        if (canBePlaced)
        {
            renderer.material = originalMaterial;
        }
        else
        {
            renderer.material = illegalPlacementMaterial;
        }
    }

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.sharedMaterial;

        if (illegalPlacementMaterial == null)
        {
            illegalPlacementMaterial = new Material(originalMaterial) { color = Color.red };
        }

        attackCooldown = new WaitForSeconds(trapData.attackCooldown);
        enemiesInRange = new List<Enemy>();
        attackCoroutine = Attack();
        
        trapAttackArea.EnemyMovementRegistered += OnEnemyMovementRegistered;
    }

    private void OnEnemyMovementRegistered(Enemy enemy, bool isInAttackArea)
    {
        if (isInAttackArea)
        {
            enemiesInRange.Add(enemy);

            if (enemiesInRange.Count == 1)
            {
                // There were no enemies in range before adding this one, so start the attack sequence. Otherwise the attack is already/still running
                currentEnemy = enemy;
                StartCoroutine(attackCoroutine);
            }
        }
        else
        {
            enemiesInRange.Remove(enemy);

            if (enemiesInRange.Count == 0)
            {
                // There are no more enemies in range so the attack sequence needs to be stopped
                StopCoroutine(attackCoroutine);
            }
            else
            {
                FindNextTarget();
            }
        }
    }

    private IEnumerator Attack()
    {
        while (currentEnemy != null)
        {
            Attack(currentEnemy);

            yield return attackCooldown;
        }
    }   
    
    private void Attack(Enemy enemy)
    {
        // TODO attack the enemy
    }

    /// <summary>
    /// Sets the current enemy to the one that is closest to its target, i.e. the one that has traveled the most distance.
    /// </summary>
    private void FindNextTarget()
    {
        currentEnemy = enemiesInRange[0];
        int countOfEnemiesInRange = enemiesInRange.Count;

        for (int i = 0; i < countOfEnemiesInRange; i++)
        {
            if (enemiesInRange[i].DistanceToTarget < currentEnemy.DistanceToTarget)
            {
                currentEnemy = enemiesInRange[i];
            }
        }
    }
}
