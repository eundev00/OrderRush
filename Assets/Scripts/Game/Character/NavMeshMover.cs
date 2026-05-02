using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class NavMeshMover : MonoBehaviour
{
    [NotNull][SerializeField] NavMeshAgent _agent;
    [SerializeField] float _normalSpeed = 3.5f;
    [SerializeField] float _slowSpeed = 1.5f;

    public void SetSlowSpeed() => _agent.speed = _slowSpeed;
    public void SetNormalSpeed() => _agent.speed = _normalSpeed;
    public Vector3 Velocity => _agent.velocity;

    void Awake()
    {
        _agent.speed = _normalSpeed;
    }

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