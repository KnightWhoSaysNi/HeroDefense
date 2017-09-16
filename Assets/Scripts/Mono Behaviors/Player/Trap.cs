using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Trap : Placeable
{
    public TrapData trapData;
    public float minTimeInAttackState;
    [Tooltip("Time it takes for the attack to connect with the target enemy after the trap has fired. " +
        "This should be a very small value so the enemy doesn't leave the attack area by the time it gets hit." +
        "This should sync with the attack animation.")]
    [Range(0,1)] public float attackHitDelay;
    protected TrapState state;
    protected WaitForSeconds attackCooldown;
    protected WaitForSeconds minWaitInAttackState;
    protected WaitForSeconds waitAttackHitDelay;
    /// <summary>
    /// <see cref="attackCooldown"/> time minus <see cref="minWaitInAttackState"/> seconds.
    /// </summary>
    protected WaitForSeconds restOfAttackCooldown;

    [Space(10)]
    public TrapAttackArea trapAttackArea;
    protected List<Enemy> enemiesInRange;
    /// <summary>
    /// All other enemies, besides the current enemy, getting hit by the trap. 
    /// This is used only for multiple targets and area of effect target systems.
    /// </summary>
    protected List<Enemy> affectedEnemies;
    protected Collider[] aoeColliders;
    protected Enemy currentEnemy;
    protected Coroutine attackCoroutine;   

    [Space(10)]
    public Material illegalPlacementMaterial;
    protected Material originalMaterial;
    protected new Renderer renderer;    

    protected virtual void Start() // TODO check if this needs to be in Awake instead
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

        enemiesInRange = new List<Enemy>();
        affectedEnemies = new List<Enemy>();
        aoeColliders = new Collider[trapData.hitAllTargetsInRange ? 300 : trapData.maxNumberOfTargets]; // ADD TO CONST arbitrary number chosen right now
        attackCooldown = new WaitForSeconds(trapData.attackCooldown);
        minWaitInAttackState = new WaitForSeconds(minTimeInAttackState);
        restOfAttackCooldown = new WaitForSeconds(trapData.attackCooldown - minTimeInAttackState);
        waitAttackHitDelay = new WaitForSeconds(attackHitDelay);

        trapAttackArea.EnemyMovementRegistered += OnEnemyMovementRegistered;
        Enemy.EnemyDied += OnEnemyDied;
    }

    /// <summary>
    /// A simple visual representation of the validity of the trap's placement. If it cannot be placed it changes its material 
    /// to the specified illegal placement material, and sets it back to original material if it can be placed.
    /// </summary>
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


    protected virtual void OnEnemyDied(Enemy enemy, Collider enemyCollider)
    {
        if (enemy == currentEnemy)
        {
            currentEnemy = null;
            UpdateCurrentEnemy();
        }
    }

    /// <summary>
    /// If the specified enemy has entered the attack area it is added to the list of enemies in range to be attacked when possible.
    /// If the specified enemy has left the attack area it is removed from the list of enemies in range so it cannot be attacked.
    /// </summary>
    /// <param name="enemy">Enemy whose movement through the trap attack area has been recorded.</param>
    /// <param name="isInAttackArea">True if the enemy has just entered the attack area, false if it just left it. </param>
    protected virtual void OnEnemyMovementRegistered(Enemy enemy, bool isInAttackArea)
    {
        if (isInAttackArea)
        {
            // Enemy came in the attack area
            enemiesInRange.Add(enemy);

            if (enemiesInRange.Count == 1)
            {
                // There were no enemies in range before adding this one, so start the attack sequence. Otherwise the attack is already running
                currentEnemy = enemy;
                attackCoroutine = StartCoroutine(Attack());
            }
        }
        else
        {
            // Enemy left the attack area
            enemiesInRange.Remove(enemy);

            if (currentEnemy == enemy)
            {
                UpdateCurrentEnemy();
            }            
        }
    }

    /// <summary>
    /// Attacks enemies while there are enemies in range to attack. Goes from attack state to normal state based on the trap data.
    /// </summary>
    protected virtual IEnumerator Attack()
    {
        // While there is a current enemy, attack it every attackCooldown seconds
        while (currentEnemy != null)
        {
            if (state != TrapState.AttackState)
            {
                GoToAttackState();
            }

            // This is used so that enemies don't take damage in the same frame that the trap attacks, unless it is supposed to be instantaneous.
            // There should be a small delay after starting the attack animation and atually hitting (and dealing damage to) the enemy - perhaps the animation time
            yield return waitAttackHitDelay;

            // There is a chance that all enemies in range were killed during the above delay and there is no one to deal damage to
            if (currentEnemy == null)
            {
                yield return minWaitInAttackState;
                break;
            }

            AttackEnemies();

            if (trapData.attackMode == AttackMode.ContinuousAttack)
            {
                // In continuous attack mode the trap fires after attack cooldown and stays in attack state
                yield return attackCooldown;
            }
            else
            {
                // Attack mode is single attack so after going into the attack state the trap stays in that state
                // for minTimeInAttackState, then goes to normal state and then waits for the rest of the attack cooldown time
                // before attacking again. (this way the attack cooldown stays the same)
                yield return minWaitInAttackState;
                GoToNormalState();
                yield return restOfAttackCooldown;
            }
                        
            if (currentEnemy == null)
            {
                // Current enemy is either out of range or it has died  
                UpdateCurrentEnemy();
            }
        }

        // At this point there are no more enemies to attack
        GoToNormalState();
    }

    /// <summary>
    /// Attacks the current enemy and if the targeting system is not 'single target' it attacks all other valid targets as well.
    /// </summary>
    protected virtual void AttackEnemies()
    {  
        switch (trapData.targetingSystem)
        {
            case TargetingSystem.SingleTarget:
                AttackSingleEnemy();                
                break;
            case TargetingSystem.MultipleTargets:
                UpdateEnemiesInRange();
                FindMultipleTargetEnemies();
                AttackMultipleEnemies();
                break;
            case TargetingSystem.AreaOfEffect:
                UpdateEnemiesInRange();
                FindAreaTargetEnemies();
                AttackAoeEnemies();
                break;            
        }
    }

    /// <summary>
    /// Attacks the current enemy;
    /// </summary>
    protected virtual void AttackSingleEnemy()
    {        
        currentEnemy.RegisterAttack(trapData.damage, trapData.damageType);        
    }

    /// <summary>
    /// Finds multiple enemies affected by the attack, besides the current enemy, based on the trap data.
    /// </summary>
    /// <remarks>This is called only for traps that have multiple targets targeting system.</remarks>
    protected virtual void FindMultipleTargetEnemies()
    {       
        if (!trapData.hitAllTargetsInRange)
        {
            affectedEnemies.Clear();            

            for (int i = 0; i < enemiesInRange.Count && i < trapData.maxNumberOfTargets; i++)
            {
                // TODO Takes the first 'max number of targets' enemies from the list of enemies in range.
                // But their position in regard to the trap position and rotation, and their destination may be random at this point. 
                // If this isn't the desired way override this method in a derived class
                affectedEnemies.Add(enemiesInRange[i]);
            }            
        }
    }


    /// <summary>
    /// Finds enemies affected by the aoe attack close to the main target - currentEnemy.    
    /// </summary>
    /// <remarks>This is called only for traps that have area of effect targeting system.</remarks>
    protected virtual void FindAreaTargetEnemies()
    {
        affectedEnemies.Clear();
        
        int numberOfColliders = Physics.OverlapSphereNonAlloc(currentEnemy.transform.position, trapData.areaOfEffectRange, aoeColliders, trapAttackArea.enemyLayerMask);

        for (int i = 0; i < numberOfColliders; i++)
        {
            Enemy affectedEnemy = aoeColliders[i].GetComponent<Enemy>();

            if (affectedEnemy != null)
            {
                affectedEnemies.Add(affectedEnemy);
            }
        }
    }

    // TODO delete this
    private void OnDrawGizmos()
    {
        if (currentEnemy!=null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentEnemy.transform.position, trapData.areaOfEffectRange);
        }
    }

    /// <summary>
    /// Attacks multiple enemies. Either all enemies in range or a number of them, based on the trap data.
    /// </summary>
    protected virtual void AttackMultipleEnemies()
    {       
        if (trapData.hitAllTargetsInRange)
        {
            for (int i = 0; i < enemiesInRange.Count; i++)
            {
                enemiesInRange[i].RegisterAttack(trapData.damage, trapData.damageType);
            }
        }
        else // hit only a number of affected enemies
        {
            for (int i = 0; i < affectedEnemies.Count; i++)
            {
                affectedEnemies[i].RegisterAttack(trapData.damage, trapData.damageType);
            }
        }
    }

    /// <summary>
    /// Attacks enemies around the current enemy and deals area damage to them.
    /// </summary>
    protected virtual void AttackAoeEnemies()
    {
        for (int i = 0; i < affectedEnemies.Count; i++)
        {
            if (affectedEnemies[i] == currentEnemy)
            {
                currentEnemy.RegisterAttack(trapData.damage, trapData.damageType);
            }
            else
            {
                affectedEnemies[i].RegisterAttack(trapData.areaDamage, trapData.damageType);
            }
        }
    }

    // TODO create an event whose handler this will be
    protected virtual void OnAttackConnected(Enemy enemy, float damage)
    {
        enemy.RegisterAttack(damage, trapData.damageType);
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
    /// Goes through all enemies in range and sets the current enemy to the one that is closest to its target.
    /// If there are no more enemies in range the current enemy is set to null and trap has no targets to attack.
    /// </summary>
    protected virtual void UpdateCurrentEnemy()
    {
        UpdateEnemiesInRange();

        if (enemiesInRange.Count == 0)
        {
            currentEnemy = null;
        }
        else
        {
            currentEnemy = enemiesInRange[0];

            // At this point the current enemy might not be the one that is closest to finishing the level
            // so the list of enemies in range is traversed to find a better target
            for (int i = 0; i < enemiesInRange.Count; i++)
            {
                if (enemiesInRange[i].moveAgent.DistanceToTarget < currentEnemy.moveAgent.DistanceToTarget)
                {
                    currentEnemy = enemiesInRange[i];
                }
            }
        }
    }

    /// <summary>
    /// Goes through the list of enemies in range and removes the ones that were killed.
    /// </summary>
    private void UpdateEnemiesInRange()
    {
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i].isDead)
            {
                enemiesInRange.RemoveAt(i);
            }
        }
    }
}

public enum TrapState { NormalState, AttackState }
