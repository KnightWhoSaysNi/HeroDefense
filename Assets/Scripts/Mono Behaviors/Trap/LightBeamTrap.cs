using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightBeamTrap : Trap
{    
    private LineRenderer beam;

    protected new void Awake()
    {
        base.Awake();

        if (trapData.targetingSystem != TargetingSystem.SingleTarget)
        {
            throw new UnityException($"{gameObject.name}'s targeting system is not set to SingleTarget. LightBeamTraps can only be SingleTarget traps.");
        }

        beam = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (state == TrapState.AttackState)
        {
            if (currentEnemy != null)
            {                
                beam.SetPosition(1, currentEnemy.transform.position);
            }
        }
    }

    protected override void OnTrapPlaced()
    {
        base.OnTrapPlaced();

        beam.SetPosition(0, attackPosition.position);
    }
}
