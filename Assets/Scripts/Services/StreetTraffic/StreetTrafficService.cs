using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Services.UpdateService;
using UnityEngine;

public class StreetTrafficService : IUpdatable, IDisposable
{
    private readonly IUpdateSubscriptionService _updateService;
    private readonly SpawnFactory _spawnFactory;
    private readonly ILevelService _levelService;
    private readonly IGameDataService _gameDataService;

    private IDisposable _dayEndedSubscription;
    private IDisposable _gameCleanupSubscription;

    private readonly List<GameObject> _activeMovers = new();
    private StreetTrafficData _data;
    private LevelContext _levelContext;
    private bool _isActive;
    private bool _isInitialized;

    private float _vehicleSpawnTimer;
    private float _walkerSpawnTimer;
    private float _nextVehicleSpawnInterval;
    private float _nextWalkerSpawnInterval;

    public StreetTrafficService(
        IUpdateSubscriptionService updateService,
        SpawnFactory spawnFactory,
        ILevelService levelService,
        IGameDataService gameDataService,
        ISubscriber<DayEndedEvent> dayEndedSubscriber,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber)
    {
        _updateService = updateService;
        _spawnFactory = spawnFactory;
        _levelService = levelService;
        _gameDataService = gameDataService;

        _dayEndedSubscription = dayEndedSubscriber.Subscribe(OnDayEnded);
        _gameCleanupSubscription = gameCleanupSubscriber.Subscribe(OnGameCleanup);
    }

    public void Initialize()
    {
        _data = _gameDataService.StreetTrafficData;
        if (_data == null)
            return;

        _levelContext = _levelService.Context;
        if (_levelContext == null || _levelContext.TrafficContext == null)
            return;

        _updateService.RegisterUpdatable(this);
        ResetTimers();
        _isActive = true;
        _isInitialized = true;
    }

    public void ManagedUpdate()
    {
        if (!_isActive || _data == null)
            return;

        _vehicleSpawnTimer += Time.deltaTime;
        if (_vehicleSpawnTimer >= _nextVehicleSpawnInterval)
        {
            _vehicleSpawnTimer = 0f;
            _nextVehicleSpawnInterval = UnityEngine.Random.Range(
                _data.VehicleSpawnIntervalMin, _data.VehicleSpawnIntervalMax);
            SpawnVehicle().Forget();
        }

        _walkerSpawnTimer += Time.deltaTime;
        if (_walkerSpawnTimer >= _nextWalkerSpawnInterval)
        {
            _walkerSpawnTimer = 0f;
            _nextWalkerSpawnInterval = UnityEngine.Random.Range(
                _data.WalkerSpawnIntervalMin, _data.WalkerSpawnIntervalMax);
            SpawnWalker().Forget();
        }
    }

    private async UniTaskVoid SpawnVehicle()
    {
        var roadPaths = _levelContext.TrafficContext.RoadPaths;
        if (roadPaths == null || roadPaths.Length == 0)
            return;

        var prefabKeys = _data.VehiclePrefabKeys;
        if (prefabKeys == null || prefabKeys.Length == 0)
            return;

        var selectedPath = roadPaths[UnityEngine.Random.Range(0, roadPaths.Length)];
        Vector3[] positions = selectedPath.GetPositions();
        if (positions.Length < 2)
            return;

        string prefabKey = prefabKeys[UnityEngine.Random.Range(0, prefabKeys.Length)];
        float speed = UnityEngine.Random.Range(_data.VehicleSpeedMin, _data.VehicleSpeedMax);

        var mover = await _spawnFactory.Create<TrafficWaypointMover>(
            PrefabKeys.GetPrefabPath(prefabKey), positions[0], _levelContext.transform);

        if (mover == null)
            return;

        mover.OnDestroyed += OnMoverDestroyed;
        _activeMovers.Add(mover.gameObject);
        mover.Initialize(positions, speed);
    }

    private async UniTaskVoid SpawnWalker()
    {
        var boundaryPoints = _levelContext.TrafficContext.WalkerBoundaryPoints;
        if (boundaryPoints == null || boundaryPoints.Length < 2)
            return;

        var prefabKeys = _data.WalkerPrefabKeys;
        if (prefabKeys == null || prefabKeys.Length == 0)
            return;

        int spawnIndex = UnityEngine.Random.Range(0, boundaryPoints.Length);
        int destIndex = spawnIndex;
        while (destIndex == spawnIndex)
            destIndex = UnityEngine.Random.Range(0, boundaryPoints.Length);

        Vector3 spawnPosition = boundaryPoints[spawnIndex].position;
        Vector3 destination = boundaryPoints[destIndex].position;

        string prefabKey = prefabKeys[UnityEngine.Random.Range(0, prefabKeys.Length)];
        float speed = UnityEngine.Random.Range(_data.WalkerSpeedMin, _data.WalkerSpeedMax);

        var mover = await _spawnFactory.Create<TrafficNavMeshMover>(
            PrefabKeys.GetPrefabPath(prefabKey), spawnPosition, _levelContext.transform);

        if (mover == null)
            return;

        mover.OnDestroyed += OnMoverDestroyed;
        _activeMovers.Add(mover.gameObject);
        mover.Initialize(speed, destination);
    }

    private void OnMoverDestroyed(GameObject go)
    {
        _activeMovers.Remove(go);
    }

    private void OnDayEnded(DayEndedEvent evt)
    {
        _isActive = false;
    }

    private void OnGameCleanup(GameCleanupEvent evt)
    {
        DestroyAllMovers();
        ResetTimers();
        _isActive = true;
    }

    private void DestroyAllMovers()
    {
        for (int i = _activeMovers.Count - 1; i >= 0; i--)
        {
            var go = _activeMovers[i];
            if (go == null)
                continue;

            var waypointMover = go.GetComponent<TrafficWaypointMover>();
            if (waypointMover != null)
                waypointMover.OnDestroyed = null;

            var navMeshMover = go.GetComponent<TrafficNavMeshMover>();
            if (navMeshMover != null)
                navMeshMover.OnDestroyed = null;

            UnityEngine.Object.Destroy(go);
        }
        _activeMovers.Clear();
    }

    private void ResetTimers()
    {
        _vehicleSpawnTimer = 0f;
        _walkerSpawnTimer = 0f;
        _nextVehicleSpawnInterval = UnityEngine.Random.Range(
            _data.VehicleSpawnIntervalMin, _data.VehicleSpawnIntervalMax);
        _nextWalkerSpawnInterval = UnityEngine.Random.Range(
            _data.WalkerSpawnIntervalMin, _data.WalkerSpawnIntervalMax);
    }

    public void Dispose()
    {
        if (_isInitialized)
            _updateService.UnregisterUpdatable(this);

        _dayEndedSubscription?.Dispose();
        _gameCleanupSubscription?.Dispose();
        DestroyAllMovers();
    }
}
