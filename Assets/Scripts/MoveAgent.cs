using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveAgent : MonoBehaviour
{
    public Transform[] waypoints;

    private NavMeshAgent agent;
    private int waypointIndex;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waypointIndex = ++waypointIndex % 2;
            agent.SetDestination(waypoints[waypointIndex].position);
        }
    }
}
