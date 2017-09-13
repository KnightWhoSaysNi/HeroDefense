using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(MoveAgent))]
public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;
    [HideInInspector] public MoveAgent moveAgent;

    public Material hitMaterial;
    protected new Renderer renderer;
    protected Material originalMaterial;

    [HideInInspector] public EnemyState state;
    [HideInInspector] public int numberOfAttackers;
    protected HashSet<Trap> continuousAttackers;
    protected WaitForSeconds minWaitInAttackedState;
    protected bool isHit;    

    public virtual void RegisterAttack(float damage, DamageType damageType, AttackMode attackMode, Trap attackingTrap = null) // TODO Incorporate different damage types into this method
    {
        isHit = true;

        if (attackMode == AttackMode.ContinuousAttack && attackingTrap != null)
        {
            continuousAttackers.Add(attackingTrap);
        }

        if (numberOfAttackers != 0)
        {
            // Enemy is already being attacked so just take damage // TODO maybe indicate that the enemy is hit again
            enemyData.currentHealth -= damage; // TODO Write a check for hp dropping below 0            
        }
        else 
        {
            // Enemy isn't being attacked at the moment, this is the first attack/the start of a continuous attack
            StartCoroutine(TakeDamage(damage));
        }        
    }

    protected virtual void Awake()
    {        
        state = EnemyState.NormalState;
        moveAgent = GetComponent<MoveAgent>();

        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.sharedMaterial;
        minWaitInAttackedState = new WaitForSeconds(0.1f); // ADD TO CONST 
    }

    protected virtual void Start()
    {
                
    }

    protected virtual IEnumerator TakeDamage(float damage)
    {
        enemyData.currentHealth -= damage;
           
        // While there are still possible attackers once per frame check if they have attacked or not
        while (numberOfAttackers > 0)
        {
            while (isHit)
            {
                // Enemy was hit since last iteration so it goes into attacked state and stays in it for minWaitInAttackedState seconds.
                // isHit is set to false to stop an infinite cycle, but during the minWaitInAttackedState seconds this variable can be set to 
                // true outside of this coroutine if the enemy is attacked again
                isHit = false;
                if (state != EnemyState.AttackedState)
                {
                    GoToAttackedState();

                    while (continuousAttackers.Count > 0)
                    {
                        // While there is at least 1 attacker with a continuous attack mode there is no need to go back and forth 
                        // between attacked state and normal state. The enemy should just stay in attacked state and every
                        // minWaitInAttackedState seconds check the condition for staying in that state
                        yield return minWaitInAttackedState; // TODO in the last iteration this will cause a double wait time. resolve that mayhaps?
                    }
                }
                yield return minWaitInAttackedState;
            }

            // Not being attacked at this moment so go back to normal state, if not already in it
            if (state != EnemyState.NormalState)
            {
                GoToNormalState();      
            }

            // Continue this loop in the next frame
            yield return null;
        }
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
