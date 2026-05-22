using System.Collections; // Imports coroutine support
using Unity.AI.Navigation; // Imports surface tools
using UnityEngine; // Imports Unity engine
using UnityEngine.AI; // Imports navigation tools

public class EnemyMovement : MonoBehaviour // Defines enemy movement
{ // Opens enemy scope
    public Transform player; // Tracks player target
    NavMeshAgent navMeshAgent; // Stores navigation agent

    IEnumerator Start() // Initializes enemy movement
    { // Opens start scope
        navMeshAgent = GetComponent<NavMeshAgent>(); // Caches navigation agent
        NavMeshSurface surface = FindObjectOfType<NavMeshSurface>(); // Finds navigation surface
        if (surface) surface.BuildNavMesh(); // Builds runtime mesh
        yield return null; // Waits one frame
        if (!navMeshAgent) yield break; // Stops missing agent
        if (!navMeshAgent.enabled) navMeshAgent.enabled = true; // Enables navigation agent
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas)) navMeshAgent.Warp(hit.position); // Snaps onto mesh
    } // Ends start scope

    void Update() // Updates enemy pursuit
    { // Opens update scope
        if (player && navMeshAgent && navMeshAgent.isOnNavMesh) navMeshAgent.SetDestination(player.position); // Chases player position
    } // Ends update scope
} // Ends enemy scope
