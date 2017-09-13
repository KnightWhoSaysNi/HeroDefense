using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Trap : Placeable
{
    public TrapData trapData;
    public float minTimeInAttackState;
    protected TrapState state;
    protected WaitForSeconds attackCooldown;
    protected WaitForSeconds minWaitInAttackState;
    /// <summary>
    /// <see cref="attackCooldown"/> time minus <see cref="minWaitInAttackState"/> seconds.
    /// </summary>
    protected WaitForSeconds restOfAttackCooldown; 

    public TrapAttackArea trapAttackArea;
    protected List<Enemy> enemiesInRange;
    protected Enemy currentEnemy;
    protected IEnumerator attackCoroutine;

    public Material illegalPlacementMaterial;
    protected Material originalMaterial;
    protected new Renderer renderer;

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

    protected virtual void Start()
    {
        state = TrapState.NormalState;

        if (trapData == null)
        {
            // TODO Resolve this situation. Throw an exception or use a blank trap data
        }

        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.sharedMaterial;
        if (illegalPlacementMaterial == null)
        {
            illegalPlacementMaterial = new Material(originalMaterial) { color = Color.red };
        }
        
        attackCooldown = new WaitForSeconds(trapData.attackCooldown);
        minWaitInAttackState = new WaitForSeconds(minTimeInAttackState);
        restOfAttackCooldown = new WaitForSeconds(trapData.attackCooldown - minTimeInAttackState);
        enemiesInRange = new List<Enemy>();
        attackCoroutine = Attack();
        
        trapAttackArea.EnemyMovementRegistered += OnEnemyMovementRegistered;
    }

    protected virtual void OnEnemyMovementRegistered(Enemy enemy, bool isInAttackArea)
    {
        if (isInAttackArea)
        {
            enemiesInRange.Add(enemy);            

            if (enemiesInRange.Count == 1)
            {
                // There were no enemies in range before adding this one, so start the attack sequence. Otherwise the attack is already running
                currentEnemy = enemy;
                StartCoroutine(attackCoroutine);
            }
        }
        else 
        {
            // Enemy left the attack area
            enemiesInRange.Remove(enemy);

            if (enemiesInRange.Count == 0)
            {
                // There are no more enemies in range so the attack sequence needs to be stopped                
                currentEnemy = null;
                StopCoroutine(attackCoroutine); // At the moment this isn't necessary, but it might be for derived classes
            }
            else
            {
                FindNextTarget();
            }

            // At this point the enemy isn't being attack any more
            enemy.numberOfAttackers--;
        }
    }

    protected virtual IEnumerator Attack()
    {
        currentEnemy.numberOfAttackers++;

        // While there is a current enemy attack it every attack cooldown seconds
        while (currentEnemy != null)
        {
            currentEnemy.RegisterAttack(trapData.damage, trapData.damageType, trapData.attackMode );

            if (state != TrapState.AttackState)
            {
                GoToAttackState();
            }

            if (trapData.attackMode == AttackMode.ContinuousAttack)
            {
                yield return attackCooldown;
            }
            else
            {
                // Attack mode is single attack so after going into the attack state the trap stays in that state
                // for minTimeInAttackState, then goes to normal state and then waits for the rest of the attack cooldown time
                // before attacking again. (this way the attack cooldown is the same regardless of the attack mode)
                yield return minWaitInAttackState;
                GoToNormalState();
                yield return restOfAttackCooldown;                
            }            
        }
    }   

    protected virtual void GoToAttackState()
    {
        state = TrapState.AttackState;
        // TODO Play attack animation
    }

    protected virtual void GoToNormalState()
    {
        state = TrapState.NormalState;
        // TODO Play idle animation or signal the stop of attack animation
    }
    

    /// <summary>
    /// Sets the current enemy to the one that is closest to its target, i.e. the one that has traveled the most distance.
    /// </summary>
    protected virtual void FindNextTarget()
    {
        currentEnemy = enemiesInRange[0];
        int countOfEnemiesInRange = enemiesInRange.Count;

        for (int i = 0; i < countOfEnemiesInRange; i++)
        {
            //if (enemiesInRange[i].DistanceToTarget < currentEnemy.DistanceToTarget)
            //{
            //    currentEnemy = enemiesInRange[i];
            //}
        }
    }
}

public enum TrapState { NormalState, AttackState }
