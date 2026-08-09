using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class NavMeshMover : MonoBehaviour
{
    [NotNull][SerializeField] NavMeshAgent _agent;
    [SerializeField] float _normalSpeed = 4.0f;
    [SerializeField] float _slowSpeed = 3.0f;

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

        float stuckTime = 0f;
        const float stuckThreshold = 1f;

        await UniTask.WaitUntil(() =>
        {
            if (IsArrived()) return true;

            if (!_agent.pathPending && _agent.velocity.sqrMagnitude < 0.01f)
            {
                stuckTime += Time.deltaTime;
                if (stuckTime >= stuckThreshold) return true;
            }
            else
            {
                stuckTime = 0f;
            }

            return false;
        }, cancellationToken: ct);
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