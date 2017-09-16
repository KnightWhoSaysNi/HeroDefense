using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MoveAgent))]
public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    [HideInInspector] public MoveAgent moveAgent;

    public Material hitMaterial;
    protected new Renderer renderer;
    protected Material originalMaterial;

     public EnemyState state;       // TODO hide in inspector
     public bool isDead;            // TODO hide in inspector
     public float currentHealth;    // TODO hide in inspector
    protected Coroutine updateStateCoroutine; // TODO delete this if it's not being called in StopCoroutine
    protected WaitForSeconds minWaitInAttackedState;
    protected bool isHit; 

    private new Collider collider;

    public static event System.Action<Enemy, Collider> EnemyDied;

    /// <summary>
    /// Tells the enemy that it has been attacked and provides information about that attack.
    /// </summary>
    public virtual void RegisterAttack(float damage, DamageType damageType) 
    {
        isHit = true;

        if (state == EnemyState.NormalState)
        {
            // Enemy isn't in attacked state at the moment, so either there were no recent attackers
            // or the current attacker's attack cooldown is greater than the minWaitInAttackedState time            
            updateStateCoroutine = StartCoroutine(UpdateState());
        }       

        TakeDamage(damage, damageType);
    }

    protected virtual void Awake()
    {
        state = EnemyState.NormalState;        
        moveAgent = GetComponent<MoveAgent>();

        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.sharedMaterial;
        minWaitInAttackedState = new WaitForSeconds(0.1f); // ADD TO CONST 

        currentHealth = enemyData.maxHealth;
        collider = GetComponent<Collider>();
    }

    /// <summary>
    /// Calculates the final damage taken from the hit based on specified parameters and after taking into account the enemy data (like armor).
    /// </summary>
    /// <param name="damage">Base damage received.</param>
    protected virtual void TakeDamage(float damage, DamageType damageType)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            //print(currentHealth);
            Die();
        }
    }

    int numberOfTimesDieWasCalled;
    protected virtual void Die()
    {
        isDead = true;

        numberOfTimesDieWasCalled++;
        if (collider == null)
        {
            print("Collider is null and Die was called "+numberOfTimesDieWasCalled.ToString()+" times. Current fuckign health is: "+currentHealth);
        }
        
        EnemyDied?.Invoke(this, collider);
        // TODO Play death animation 
        
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Changes state based on whether or not the enemy is hit
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator UpdateState()
    {
        while (isHit)
        {
            // Enemy was hit since last iteration so it goes into attacked state and stays in it for minWaitInAttackedState.
            // isHit is set to false to stop an infinite cycle, but during the minWaitInAttackedState this variable can be set to 
            // true outside of this coroutine if the enemy is attacked again
            isHit = false;
            if (state != EnemyState.AttackedState)
            {
                GoToAttackedState();
            }
            yield return minWaitInAttackedState;
        }

        // Not being attacked at this moment so go back to normal state
        GoToNormalState();        
    }


    protected virtual void GoToAttackedState()
    {
        state = EnemyState.AttackedState;
        renderer.material = hitMaterial;
    }

    protected virtual void GoToNormalState()
    {
        state = EnemyState.NormalState;
        renderer.material = originalMaterial;
    }
}

public enum EnemyState { NormalState, AttackedState }
