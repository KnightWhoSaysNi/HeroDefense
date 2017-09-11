using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapData : ScriptableObject
{
    public int cost;
    //public float range;
    public float damage;
    /// <summary>
    /// How often this trap attacks. In seconds.
    /// </summary>
    public float attackCooldown;
    public TargetSystem targetSystem;
    public DamageType damageType;
    public AttackMode attackMode;
    
}

public enum TargetSystem { SingleTarget, MultipleTargets, AreaOfEffect }
public enum DamageType { Normal, Fire, Lightning, Poison }
public enum AttackMode { SingleAttack, ContinuousAttack }

