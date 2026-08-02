using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using UnityEngine;
using VContainer;

public class LevelContext : MonoBehaviour
{
    [NotNull][SerializeField] Transform _interactablesRoot;
    [NotNull][SerializeField] Transform[] _tablePoints;
    [NotNull][SerializeField] Transform _spawnPoint;
    [NotNull][SerializeField] Transform _waitingPoint;
    [NotNull][SerializeField] Transform[] _staffIdlePoints;
    [SerializeField] private GameObject _rainRoot;
    [SerializeField] private GameObject _nightLightRoot;

    private IDisposable _rainSubscription;
    private IDisposable _nightLightSubscription;

    public List<DiningTable> DiningTables { get; private set; }
    public ServingCounter[] ServingCounters { get; private set; }
    public Counter[] KitchenCounters { get; private set; }
    public Transform SpawnPoint => _spawnPoint;
    public Transform WaitingPoint => _waitingPoint;
    public Transform[] StaffIdlePoints => _staffIdlePoints;

    [Inject]
    public void Construct(ISubscriber<RainEvent> rainSubscriber, ISubscriber<NightLightEvent> nightLightSubscriber)
    {
        _rainSubscription = rainSubscriber.Subscribe(e => SetRain(e.IsRainy));
        _nightLightSubscription = nightLightSubscriber.Subscribe(e => SetNightLight(e.IsNight));
    }

    void Awake()
    {
        SetNightLight(false);

        DiningTables = new List<DiningTable>(_interactablesRoot.GetComponentsInChildren<DiningTable>());

        ServingCounters = _interactablesRoot.GetComponentsInChildren<ServingCounter>();

        KitchenCounters = _interactablesRoot.GetComponentsInChildren<Counter>()
            .Where(c => c is not ServingCounter)
            .ToArray();
    }

    public Transform GetNextTableSpawnPoint()
    {
        int index = DiningTables.Count;
        if (index < 0 || index >= _tablePoints.Length)
            return null;

        return _tablePoints[index];
    }

    public void AddDiningTable(DiningTable table)
    {
        DiningTables.Add(table);
    }

    private void SetRain(bool on)
    {
        if (_rainRoot != null)
            _rainRoot.SetActive(on);
    }

    private void SetNightLight(bool on)
    {
        if (_nightLightRoot != null)
            _nightLightRoot.SetActive(on);
    }

    void OnDestroy()
    {
        _rainSubscription?.Dispose();
        _nightLightSubscription?.Dispose();
    }
}
