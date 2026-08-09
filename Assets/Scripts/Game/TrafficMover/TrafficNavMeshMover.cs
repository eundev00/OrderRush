using System;
using Services.UpdateService;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

public class TrafficNavMeshMover : MonoBehaviour, IUpdatable
{
    [SerializeField] private NavMeshAgent _agent;

    private IUpdateSubscriptionService _updateService;
    private bool _isInitialized;

    public Action<GameObject> OnDestroyed;

    [Inject]
    public void Construct(IUpdateSubscriptionService updateService)
    {
        _updateService = updateService;
    }

    public void Initialize(float speed, Vector3 destination)
    {
        _agent.speed = speed;
        _agent.enabled = true;
        _agent.SetDestination(destination);

        _updateService.RegisterUpdatable(this);
        _isInitialized = true;
    }

    public void ManagedUpdate()
    {
        if (!_isInitialized || _agent == null || !_agent.enabled)
            return;

        if (_agent.pathPending)
            return;

        if (_agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
            DestroySelf();
    }

    private void DestroySelf()
    {
        _isInitialized = false;
        _updateService?.UnregisterUpdatable(this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_isInitialized)
            _updateService?.UnregisterUpdatable(this);

        OnDestroyed?.Invoke(gameObject);
    }
}
