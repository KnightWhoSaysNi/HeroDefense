using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// TODO If there is no blocking feature refactor this class so it doesn't use a NavMeshAgent at all, 
// but some kind of a waypoint system, which should increase performanse and FPS
[RequireComponent(typeof(NavMeshAgent))]
public class MoveAgent : MonoBehaviour  // TODO Refactor
{
    public NavMeshAgent agent; 
    private Transform followTargetTransform;
    private Vector3 targetPosition;
    private Vector3 followTargetOffset;
    private Vector3 zeroVector;
    private bool hasFollowTarget;

    public float DistanceToTarget
    {
        get
        {
            return agent.remainingDistance;
        }
    }

    /// <summary>
    /// A one time action of setting the destination for the NavMeshAgent to the specified vector3.
    /// </summary>
    /// <param name="targetPosition">New target for the NavMeshAgent.</param>
    public void SetTarget(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        StopAllCoroutines();
        StartCoroutine(SetTarget());
    }

    /// <summary>
    /// Sets the NavMeshAgent to constantly update his current destination, i.e. to follow the specified target transform.
    /// </summary>
    /// <param name="followTargetTransform">Transform of the object to follow.</param>
    public void FollowTarget(Transform followTargetTransform)
    {
        hasFollowTarget = true;
        this.followTargetTransform = followTargetTransform;
        this.followTargetOffset = zeroVector;

        StopAllCoroutines();
        StartCoroutine(FollowTarget());
    }

    /// <summary>
    /// Sets the NavMeshAgent to constantly update his current destination, i.e. to follow the specified target transform.
    /// Also adds an offset to the position of the target transform when doing the following operation.
    /// </summary>
    /// <param name="followTargetTransform">Transform of the object to follow.</param>
    /// <param name="followTargetOffset">Offset to the target transform's position.</param>
    public void FollowTarget(Transform followTargetTransform, Vector3 followTargetOffset)
    {
        hasFollowTarget = true;
        this.followTargetTransform = followTargetTransform;
        this.followTargetOffset = followTargetOffset;

        StopAllCoroutines();
        StartCoroutine(FollowTarget());
    }

    private void Awake()
    {
        zeroVector = Vector3.zero;
    }

    /// <summary>
    /// As soon it's possible sets the new destination of the NavMeshAgent.
    /// </summary>
    private IEnumerator SetTarget()
    {
        while (agent.pathPending)
        {
            yield return null;
        }

        // In case there was a follow target
        hasFollowTarget = false;
        agent.SetDestination(targetPosition);
    }

    private IEnumerator FollowTarget()
    {
        while (hasFollowTarget)
        {
            if (followTargetTransform != null)
            {
                if (!agent.pathPending &&
                    (followTargetTransform.position + followTargetOffset) != agent.destination) // A check to see if the target object has moved                                               
                {
                    agent.SetDestination(followTargetTransform.position + followTargetOffset);
                }
            }
            else
            {
                // Transform of the target object being null could mean that the target has been destroyed
                // TODO Resolve such a scenario
                hasFollowTarget = false;
            }

            yield return null;
        }
    }
}
