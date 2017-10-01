using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MoveAgent))]
public class Enemy : MonoBehaviour // Refactor
{
    public EnemyType enemyType;
    
    public EnemyData enemyData;
    [HideInInspector]
    public MoveAgent moveAgent;

    public Renderer[] otherRenderers;
    public ParticleSystem dieParticleEffect;
    public Material hitMaterial;
    protected new Renderer renderer;
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

    private new Collider collider;
       
    public delegate void EnemyDiedHandler(Enemy enemy, Collider collider, bool hasFinishedLevel);
    public static event EnemyDiedHandler EnemyDied;

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
        
    protected virtual void Awake()
    {        
        state = EnemyState.NormalState;        
        moveAgent = GetComponent<MoveAgent>();

        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.sharedMaterial;

        currentHealth = enemyData.maxHealth;
        collider = GetComponent<Collider>();

        // TODO Change this if there is a death animation, not just the particle effect
        deathTimer = dieParticleEffect.main.duration;
    }

    protected virtual void Start()
    {
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

    /// <summary>
    /// Resets enemy so that it is ready for use next time someone calls him from the pool.
    /// </summary>
    protected virtual void OnDisable()
    {
        if (state != EnemyState.NormalState)
        {
            GoToNormalState();
        }

        isDead = false;
        isHit = false;
        currentHealth = enemyData.maxHealth;
        hitTimer = -1;
        deathTimer = dieParticleEffect.main.duration;

        UpdateRenderers();
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
}

public enum EnemyState { NormalState, AttackedState }
