using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class NavMeshMover : MonoBehaviour
{
    [SerializeField] NavMeshAgent _agent;

    public async UniTask MoveToAsync(Vector3 destination, CancellationToken ct)
    {
        _agent.SetDestination(destination);
        await UniTask.WaitUntil(() => IsArrived(), cancellationToken: ct);
    }

    public void MoveDirect(Vector3 destination)
    {
        _agent.SetDestination(destination);
    }

    public void Stop()
    {
        _agent.ResetPath();
    }

    public void EnableAgent()
    {
        _agent.enabled = true;
    }

    public void DisableAgent()
    {
        _agent.enabled = false;
    }

    public void Warp(Vector3 position)
    {
        _agent.Warp(position);
    }

    bool IsArrived()
    {
        if (_agent.pathPending) return false;
        if (_agent.remainingDistance > _agent.stoppingDistance) return false;
        return true;
    }
}