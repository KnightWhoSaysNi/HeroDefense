using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrapData")]
public class TrapData : ScriptableObject // TODO Break this into several classes when Trap is refactored
{
    public float damage;
    /// <summary>
    /// How often this trap attacks. In seconds.
    /// </summary>
    [Tooltip("How often this trap attacks. In seconds. This should be synced with the attack animation.")]
    public float attackCooldown;
    public TargetingSystem targetingSystem;
    public DamageType damageType;
    public AttackMode attackMode;
    
    public float attackRange; 
    /// <summary>
    /// If this is set to false max number of targets is used, otherwise that variable is ignored.
    /// </summary>
    public bool hitAllTargetsInRange;    
    public int maxNumberOfTargets;     
    public float areaOfEffectRange;
    public float areaDamage;
}

public enum TargetingSystem { SingleTarget, MultipleTargets, AreaOfEffect }
public enum AttackMode { SingleAttack, ContinuousAttack }
public enum DamageType { Normal, Fire, Lightning, Poison }

