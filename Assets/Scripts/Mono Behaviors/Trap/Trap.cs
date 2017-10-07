using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Trap : Placeable // TODO make this class an abstract class and create a separate derived class for each targeting system
{
    #region - Fields -
    // TODO Set all fields that don't need to be public to [SerializeField] protected/private
    [Space(10)]
    public Sprite thumbnail;

    [Space(10)]
    public TrapData trapData;
    [Tooltip("Time it takes for the attack to connect with the target enemy after the trap has fired. " +
        "This should be a very small value so the enemy doesn't leave the attack area by the time it gets hit." +
        "This should sync with the attack animation.")]
    [Range(0, 1)]
    public float attackHitDelay;
    protected TrapState state;
    protected WaitForSeconds attackCooldown;
    protected WaitForSeconds waitAttackHitDelay;
    protected bool hasWaitedForAttackHitDelay;
    protected bool isWaitingForCooldown;

    [Space(10)]
    public LayerMask obstructionLayerMask;
    public TrapAttackArea trapAttackArea;
    public bool canAttackBeObstructed; // TODO This should only be available for single target traps. (it gets A LOT more complicated for other targeting systems)
    /// <summary>
    /// This transform's position is used as origin for a ray cast towards the enemy to check if it can be attacked or if it's obstructed by something.
    /// </summary>
    public Transform attackPosition;
    protected List<Enemy> enemiesInRange;
    /// <summary>
    /// All other enemies, besides the current enemy, getting hit by the trap. 
    /// This is used only for multiple targets and area of effect target systems.
    /// </summary>
    protected List<Enemy> affectedEnemies;
    protected Collider[] aoeColliders;
    protected Enemy currentEnemy;
    protected Coroutine attackCoroutine;
    protected bool isObstructed;

    protected Animator animator;
    #endregion

    #region - Properties -
    public int Cost
    {
        get
        {
            return goldCost;
        }
    }
    #endregion

    #region - MonoBehavior methods -
    protected new void Awake()
    {
        base.Awake();

        state = TrapState.NormalState;

        enemiesInRange = new List<Enemy>();
        affectedEnemies = new List<Enemy>();
        aoeColliders = new Collider[trapData.hitAllTargetsInRange ? 300 : trapData.maxNumberOfTargets]; // ADD TO CONST 300 is arbitrary number used for testing
        attackCooldown = new WaitForSeconds(trapData.attackCooldown);
        waitAttackHitDelay = new WaitForSeconds(attackHitDelay);

        trapAttackArea.EnemyMovementRegistered += OnEnemyMovementRegistered;

        animator = GetComponent<Animator>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        enemiesInRange.Clear();
        hasWaitedForAttackHitDelay = false;
        isWaitingForCooldown = false;
        currentEnemy = null;
    }
    #endregion    

    #region - Placeable override methods -
    protected override void OnPlaced()
    {
        base.OnPlaced();
        // TODO Create and play some animation/sound
    }

    protected override void OnSold()
    {
        base.OnSold();
        GoToNormalState();
    }
    #endregion

    #region - Protected and private methods -
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
                currentEnemy = enemy;

                if (state == TrapState.NormalState)
                {
                    // There were no enemies in range before adding this one, so start the attack sequence. Otherwise the attack is already running
                    attackCoroutine = StartCoroutine(Attack());
                }
            }
        }
        else
        {
            // Enemy left the attack area
            enemiesInRange.Remove(enemy);

            if (currentEnemy == enemy)
            {
                StartCoroutine(UpdateCurrentEnemy());
            }
        }
    }

    /// <summary>
    /// Attacks enemies while there are enemies in range to attack. Goes from attack state to normal state based on the trap data.
    /// </summary>
    protected virtual IEnumerator Attack() // TODO REFACTOR !!!
    {
        while (isWaitingForCooldown)
        {
            yield return null;
        }

        // While there is a current enemy, attack it every attackCooldown seconds/frame (continuous attacks)
        while (currentEnemy != null)
        {
            while (!isPlaced)
            {
                // The trap isn't placed yet, but it still needs to keep track of all its enemies for when it gets placed
                yield return null;
            }

            // If there's a wall or some other obstacle blocking the shot at the current enemy the trap cannot attack yet
            if (canAttackBeObstructed)
            {
                CheckIfObstructed();

                // It's set up this way so that the attack coroutine doesn't lose a frame if there is no obstruction
                while (isObstructed)
                {
                    yield return null;
                    CheckIfObstructed();
                }

                // Setting it back to true for the next frame or attack iteration
                isObstructed = true;
            }

            if (state != TrapState.AttackState)
            {
                GoToAttackState();
            }

            // This is used so that enemies don't take damage in the same frame that the trap attacks, unless it is supposed to be instantaneous.
            // There should be a small delay after starting the attack animation and atually hitting (and dealing damage to) the enemy - perhaps the attack animation time
            if (attackHitDelay != 0)
            {
                if (trapData.attackMode == AttackMode.SingleAttack || !hasWaitedForAttackHitDelay)
                {
                    // SingleAttack traps wait for an attack hit delay every single attack
                    // Continuous traps wait only for the first attack, after that they attack continuously (the attack animation is in a loop)
                    yield return waitAttackHitDelay;
                    hasWaitedForAttackHitDelay = true;

                    if (currentEnemy == null)
                    {
                        // Current enemy (and all others in range) died during the attack hit delay. State and cooldown resolved in StopAttackCoroutine()
                        yield break;
                    }
                }
            }

            AttackEnemies();

            if (currentEnemy == null)
            {
                // Current enemy (and all others in range) died from the attack. State and cooldown resolved in StopAttackCoroutine()
                yield break;
            }

            if (trapData.attackMode == AttackMode.ContinuousAttack)
            {
                // In continuous attack mode the trap deals damage every frame and so the attack cooldown should be 0 (but it won't change anything if it isn't)
                yield return null;
            }
            else
            {
                GoToNormalState();

                isWaitingForCooldown = true;
                yield return attackCooldown;
                isWaitingForCooldown = false;
            }
        }

        // At this point there are no more enemies to attack
        GoToNormalState();
    }

    /// <summary>
    /// Checks if the trap has a clean shot from the <see cref="attackPosition"/> to the current enemy.
    /// </summary>
    protected virtual void CheckIfObstructed()
    {
        if (currentEnemy == null)
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(attackPosition.position, currentEnemy.transform.position - attackPosition.position, out hit, 100f, obstructionLayerMask)) // ADD TO CONST?
        {
            if (((1 << hit.transform.gameObject.layer) & trapAttackArea.enemyLayerMask) != 0)
            {
                // Enemy was hit with the ray, meaning nothing is obstructing the attack
                isObstructed = false;
            }
            else
            {
                // Enemy was not hit with the ray, meaning something is obstructing the attack
                isObstructed = true;
            }
        }
        else
        {
            // Nothing was hit with the ray. This shouldn't ever happen, but it's here just in case
            print("Ray from trap to the current enemy didn't hit anything!");
        }

    }

    /// <summary>
    /// Attacks the current enemy and if the targeting system is not 'single target' it attacks all other valid targets as well.
    /// </summary>
    protected virtual void AttackEnemies()
    {
        switch (trapData.targetingSystem)
        {
            case TargetingSystem.SingleTarget:
                AttackSingleEnemy(currentEnemy, trapData.damage);
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
    protected virtual void AttackSingleEnemy(Enemy enemy, float damage)
    {
        if (trapData.attackMode == AttackMode.ContinuousAttack)
        {
            enemy.RegisterAttack(damage * Time.deltaTime, trapData.damageType);
        }
        else
        {
            enemy.RegisterAttack(damage, trapData.damageType);
        }
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

            if (affectedEnemy != null) // TODO if affected enemy is not dead, there should be no need for a null check if the layer maks is set up correctly
            {
                affectedEnemies.Add(affectedEnemy);
            }
        }
    }

    /// <summary>
    /// Attacks multiple enemies. Either all enemies in range or a number of them, based on the trap data.
    /// </summary>
    protected virtual void AttackMultipleEnemies()
    {
        if (trapData.hitAllTargetsInRange)
        {
            // For loop goes in reverse to prevent skipping an element of the array if the count goes down (an enemy dies and reports it)
            for (int i = enemiesInRange.Count - 1; i >= 0; i--)
            {
                AttackSingleEnemy(enemiesInRange[i], trapData.damage);
            }
        }
        else // hit only a number of affected enemies
        {
            for (int i = 0; i < affectedEnemies.Count; i++)
            {
                AttackSingleEnemy(affectedEnemies[i], trapData.damage);
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
                AttackSingleEnemy(currentEnemy, trapData.damage);
            }
            else
            {
                affectedEnemies[i].RegisterAttack(trapData.areaDamage, trapData.damageType);
                AttackSingleEnemy(affectedEnemies[i], trapData.areaDamage);
            }
        }
    }

    /// <summary>
    /// Goes through all enemies in range and sets the current enemy to the one that is closest to its target.
    /// If there are no more enemies in range the current enemy is set to null and trap has no targets to attack.
    /// </summary>
    protected virtual IEnumerator UpdateCurrentEnemy()
    {
        UpdateEnemiesInRange();

        if (enemiesInRange.Count == 0)
        {
            currentEnemy = null;
            yield return StopAttackCoroutine();
        }
        else
        {
            currentEnemy = enemiesInRange[0];

            // At this point the current enemy might not be the one that is closest to finishing the level
            // so the list of enemies in range is traversed to find a better target
            for (int i = 0; i < enemiesInRange.Count; i++)
            {
                if (enemiesInRange[i].MoveAgent.DistanceToTarget < currentEnemy.MoveAgent.DistanceToTarget)
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
            if (enemiesInRange[i].IsDead)
            {
                enemiesInRange.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Stops the attack coroutine in an approprate way depending on the state of the trap and some other parameters.
    /// </summary>
    private IEnumerator StopAttackCoroutine()
    {
        // TODO Check if this is necessary. Perhaps it cannot happen
        if (attackCoroutine == null)
        {
            // Enemy walked into the attack area and died/walked out, thus causing the UpdateCurrentEnemy call before the attack coroutine could even start the first time
            print("AttackCoroutine is null. Delete this print.");
            yield break;
        }

        if (state == TrapState.AttackState)
        {
            StopCoroutine(attackCoroutine);
            GoToNormalState();

            // Only single attacks have an attack cooldown, continuous attacks ignore such values if they were erroneously set
            if (trapData.attackMode == AttackMode.SingleAttack)
            {
                // The trap attacked and since it was not waiting for the attack cooldown prior to this point it needs to wait for it now
                isWaitingForCooldown = true;
                yield return attackCooldown;
                isWaitingForCooldown = false;
            }
        }
        else if (isWaitingForCooldown)
        {
            // Trap is in normal state, but it is currently waiting for the attack cooldown and since it's impossible to know how long it has already waited
            // the Attack sequence will have to continue and finish waiting for the cooldown. Afther that the attack coroutine will stop on its own
            yield break;
        }
        else
        {
            // Trap is in normal state and it is not waiting for the attack cooldown
            StopCoroutine(attackCoroutine);
        }
    }

    protected virtual void GoToAttackState()
    {
        state = TrapState.AttackState;
        hasWaitedForAttackHitDelay = false;

        switch (trapData.attackMode)
        {
            case AttackMode.SingleAttack:
                animator.SetTrigger(Constants.TrapAnimatorAttackedTrigger); 
                break;
            case AttackMode.ContinuousAttack:
                animator.SetBool(Constants.TrapAnimatorIsAttackingBool, true); 
                break;
            default:
                throw new UnityException("Trap.GoToAttackState has code for only two attack states.");
        }
    }

    protected virtual void GoToNormalState()
    {
        state = TrapState.NormalState;

        switch (trapData.attackMode)
        {
            case AttackMode.SingleAttack:
                // By default single attack trap uses a trigger and nothing more is added here
                break;
            case AttackMode.ContinuousAttack:
                animator.SetBool(Constants.TrapAnimatorIsAttackingBool, false); 
                break;
            default:
                throw new UnityException("Trap.GoToAttackState has code for only two attack states.");
        }
    } 
    #endregion
}

public enum TrapState { NormalState, AttackState }
