using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrapData")]
public class TrapData : ScriptableObject
{
    public int cost;
    public float damage;
    /// <summary>
    /// How often this trap attacks. In seconds.
    /// </summary>
    [Tooltip("How often this trap attacks. In seconds.")]
    public float attackCooldown;
    public TargetSystem targetSystem;
    public DamageType damageType;
    public AttackMode attackMode;
    
    public float range;
    public int maxNumberOfTargets;
}

public enum TargetSystem { SingleTarget, MultipleTargets, AreaOfEffect }
public enum AttackMode { SingleAttack, ContinuousAttack }
public enum DamageType { Normal, Fire, Lightning, Poison }

