using System;
using Services.UpdateService;
using UnityEngine;
using VContainer;

public class TrafficWaypointMover : MonoBehaviour, IUpdatable
{
    private IUpdateSubscriptionService _updateService;
    private bool _isInitialized;

    private Vector3[] _path;
    private int _currentPathIndex;
    private float _speed;

    public Action<GameObject> OnDestroyed;

    [Inject]
    public void Construct(IUpdateSubscriptionService updateService)
    {
        _updateService = updateService;
    }

    public void Initialize(Vector3[] path, float speed)
    {
        _path = path;
        _speed = speed;

        transform.position = _path[0];
        _currentPathIndex = 1;
        LookAtDirection(_path[1] - _path[0]);

        _updateService.RegisterUpdatable(this);
        _isInitialized = true;
    }

    public void ManagedUpdate()
    {
        if (!_isInitialized)
            return;

        if (_currentPathIndex >= _path.Length)
        {
            DestroySelf();
            return;
        }

        Vector3 target = _path[_currentPathIndex];
        transform.position = Vector3.MoveTowards(transform.position, target, _speed * Time.deltaTime);
        LookAtDirection(target - transform.position);

        if (Vector3.Distance(transform.position, target) < 0.1f)
            _currentPathIndex++;
    }

    private void LookAtDirection(Vector3 direction)
    {
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
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
