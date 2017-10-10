using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveAgent))]
public class Enemy : MonoBehaviour, IPoolable // Refactor
{
    #region - Fields -
    [SerializeField] protected EnemyType enemyType;        
    [SerializeField] protected EnemyData enemyData;
    [SerializeField] protected MoveAgent moveAgent;
    [Space(5)]
    [SerializeField] protected Transform hitTarget;
    [SerializeField] protected Renderer[] otherRenderers;
    [SerializeField] protected ParticleSystem dieParticleEffect;
    [SerializeField] protected Material hitMaterial;
    [SerializeField] protected new Renderer renderer;
    protected Material originalMaterial;

    protected EnemyState state;
    [Range(0, 10)]
    [SerializeField]
    protected float minTimeInAttackedState = 0.1f;
    private float hitTimer = -1;
    private float deathTimer = -1;
    protected float currentHealth;
    protected bool isHit;
    protected bool isDead;

    [SerializeField] protected new Collider collider;
    protected Transform myTransform;
    #endregion

    #region - Delegates and events -
    public delegate void EnemyDiedHandler(Enemy enemy, Collider collider, bool hasFinishedLevel);
    public static event EnemyDiedHandler EnemyDied; 
    #endregion

    #region - Properties -
    public EnemyType EnemyType
    {
        get
        {
            return enemyType;
        }
    }
    public MoveAgent MoveAgent
    {
        get
        {
            return moveAgent;
        }
    }
    public Transform HitTarget
    {
        get
        {
            return hitTarget;
        }
    }

    public bool IsDead
    {
        get
        {
            return isDead;
        }
    }
    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
    }
    public float MaxHealth
    {
        get
        {
            return enemyData.maxHealth;
        }
    }
    public int Armor
    {
        get
        {
            return enemyData.armor;
        }
    }
    public int GoldReward
    {
        get
        {
            return enemyData.goldReward;
        }
    }
    public int Level
    {
        get
        {
            return enemyData.level;
        }
    }
    public int BaseExperienceReward
    {
        get
        {
            return enemyData.baseExperienceReward;
        }
    }
    public int EnergyDrain
    {
        get
        {
            return enemyData.energyDrain;
        }
    }
    #endregion

    #region - Public methods -
    /// <summary>
    /// Tells the enemy that it has been attacked and provides information about that attack.
    /// </summary>
    public virtual void RegisterAttack(float damage, DamageType damageType)
    {
        isHit = true;
        TakeDamage(damage, damageType);
    }

    public virtual void Die(bool hasFinishedLevel = false)
    {
        isDead = true;

        if (hasFinishedLevel)
        {
            deathTimer = 0;
            EnemyDied?.Invoke(this, collider, true);
        }
        else
        {
            // Enemy died
            UpdateRenderers();
            dieParticleEffect.Play();
            EnemyDied?.Invoke(this, collider, false);
        }
    }
    #endregion

    #region - MonoBehavior methods -
    protected virtual void Awake()
    {
        state = EnemyState.NormalState;
      
        myTransform = this.transform;
    }

    protected virtual void Start()
    {
        originalMaterial = renderer.sharedMaterial;
        moveAgent.agent.speed = enemyData.movementSpeed;
    }

    protected virtual void Update()
    {
        if (!isDead)
        {
            UpdateState();
        }

        if (isDead && deathTimer >= 0)
        {
            deathTimer -= Time.deltaTime;

            if (deathTimer <= 0)
            {
                // Return to the pool
                EnemyPool.Instance.ReclaimObject(enemyType, this);
            }
        }
    }

    #endregion

    #region - IPoolable interface implementation -
    public virtual void PreActivation(System.Object preActivationData)
    {
        isDead = false;
        UpdateRenderers();
        myTransform.position = myTransform.parent.position;
        myTransform.rotation = myTransform.parent.rotation;
    }
    public virtual void PostActivation(System.Object postActivationData)
    {
        moveAgent.agent.enabled = true;
        if (postActivationData != null)
        {
            Vector3 destination = (Vector3)postActivationData;
            moveAgent.SetTarget(destination);
        }
        
    }
    public virtual void PreDeactivation()
    {
        moveAgent.agent.enabled = false;
    }
    public virtual void PostDeactivation()
    {
        if (state != EnemyState.NormalState)
        {
            GoToNormalState();
        }
        
        isHit = false;
        currentHealth = enemyData.maxHealth;
        hitTimer = -1;
        // TODO Change this if there is a death animation, not just the particle effect
        deathTimer = dieParticleEffect.main.duration;        
    }
    #endregion

    #region - Protected methods -
    /// <summary>
    /// Checks if the enemy was hit. Goes into attacked state for <see cref="minTimeInAttackedState"/> seconds, after which it returns to
    /// the normal state, unless it was hit again during those <see cref="minTimeInAttackedState"/> seconds.
    /// </summary>
    protected virtual void UpdateState()
    {
        if (isHit)
        {
            isHit = false;
            hitTimer = minTimeInAttackedState;
        }

        if (hitTimer >= 0)
        {
            if (state != EnemyState.AttackedState)
            {
                GoToAttackedState();
            }

            hitTimer -= Time.deltaTime;
        }
        else if (state != EnemyState.NormalState)
        {
            GoToNormalState();
        }
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
            Die();
        }
    }    

    protected virtual void UpdateRenderers()
    {
        renderer.enabled = !isDead;
        for (int i = 0; i < otherRenderers.Length; i++)
        {
            otherRenderers[i].enabled = !isDead;
        }
    }

    protected virtual void GoToAttackedState()
    {
        state = EnemyState.AttackedState;
        moveAgent.agent.speed = enemyData.movementSpeed * 0.75f; // TEST
        renderer.material = hitMaterial;
    }

    protected virtual void GoToNormalState()
    {
        state = EnemyState.NormalState;
        moveAgent.agent.speed = enemyData.movementSpeed;
        renderer.material = originalMaterial;
    } 
    #endregion
}

public enum EnemyState { NormalState, AttackedState }
