using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using UnityEngine;

public class CustomerService : ICustomerService
{
    private readonly ILevelContextPresenter _levelPresenter;
    private readonly SpawnFactory _spawnFactory;
    private readonly IDayProgressService _dayProgressService;
    private readonly IGameDataService _gameDataService;
    private readonly ISubscriber<GameCleanupEvent> _gameCleanupSubscriber;
    private readonly ISubscriber<CustomerRemovedEvent> _customerServedSubscriber;
    private float _timeBarDuration;
    private int _customersSpawned;
    private int _maxCustomers;
    private int _maxGroupSize;
    private float _spawnClusterStrength;
    private float _lastCheckedElapsed;
    private int _servedCustomersCount;

    private readonly List<CustomerGroup> _waitingList = new();
    private readonly CompositeDisposable _disposables = new();



    public CustomerService(
        ILevelContextPresenter levelPresenter,
        SpawnFactory spawnFactory,
        IDayProgressService dayProgressService,
        IGameDataService gameDataService,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber,
        ISubscriber<CustomerRemovedEvent> customerServedSubscriber)
    {
        _levelPresenter = levelPresenter;
        _spawnFactory = spawnFactory;
        _dayProgressService = dayProgressService;
        _gameDataService = gameDataService;
        _gameCleanupSubscriber = gameCleanupSubscriber;
        _customerServedSubscriber = customerServedSubscriber;
    }

    public void Initialize()
    {
        var currentDay = _dayProgressService.CurrentDayContext;
        var daysData = _dayProgressService.CurrentDaysData;

        int dayNumber = currentDay.DayNumber;
        _timeBarDuration = daysData.GetTimeBarDuration(dayNumber);
        _maxCustomers = daysData.GetMaxCustomers(dayNumber);

        _customersSpawned = 0;
        _maxGroupSize = 2;
        _spawnClusterStrength = _gameDataService.Config.DefaultSpawnClusterStrength;
        _lastCheckedElapsed = -1f;
        _servedCustomersCount = 0;

        currentDay.TimeBarElapsed
            .Subscribe(CheckAndSpawn)
            .AddTo(_disposables);

        _gameCleanupSubscriber
            .Subscribe(_ => OnGameCleanup())
            .AddTo(_disposables);

        _customerServedSubscriber
            .Subscribe(OnCustomerRemoved)
            .AddTo(_disposables);
    }

    private void OnCustomerRemoved(CustomerRemovedEvent evt)
    {
        if (!evt.WasServed)
            return;

        _servedCustomersCount++;

        if (_servedCustomersCount >= _maxCustomers)
        {
            _dayProgressService.CompleteDay();
        }
    }

    private void OnGameCleanup()
    {
        _waitingList.Clear();
        _customersSpawned = 0;
        _lastCheckedElapsed = -1f;
        _servedCustomersCount = 0;

        var currentDay = _dayProgressService.CurrentDayContext;
        var daysData = _dayProgressService.CurrentDaysData;

        int dayNumber = currentDay.DayNumber;
        _timeBarDuration = daysData.GetTimeBarDuration(dayNumber);
        _maxCustomers = daysData.GetMaxCustomers(dayNumber);
        _spawnClusterStrength = _gameDataService.Config.DefaultSpawnClusterStrength;
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        _waitingList.Clear();
    }

    private async void CheckAndSpawn(float elapsed)
    {
        if (_customersSpawned >= _maxCustomers) return;

        float nextSpawnTime = GetSpawnTime(_customersSpawned);

        if (_lastCheckedElapsed < nextSpawnTime && elapsed >= nextSpawnTime)
        {
            _lastCheckedElapsed = elapsed;
            int groupSize = Mathf.Min(
                UnityEngine.Random.Range(1, _maxGroupSize + 1),
                _maxCustomers - _customersSpawned);
            _customersSpawned += groupSize;
            await TrySpawn(groupSize);
        }
    }

    private float GetSpawnTime(int customerIndex)
    {
        float buffer = _gameDataService.Config.SpawnBufferDuration;
        float window = _timeBarDuration - 2f * buffer;
        float progress = _maxCustomers > 1 ? customerIndex / (float)(_maxCustomers - 1) : 0f;
        return buffer + Distribute(progress) * window;
    }

    // 중반 집중 분포: strength 0 = 균등, 1 = 주간(중반) 몰림 (1 초과 시 단조성 붕괴)
    private float Distribute(float p)
    {
        float strength = Mathf.Clamp01(_spawnClusterStrength);
        return p + (strength / (2f * Mathf.PI)) * Mathf.Sin(2f * Mathf.PI * p);
    }

    private async UniTask TrySpawn(int groupSize)
    {
        if (_waitingList.Count > 0)
        {
            await SpawnToWaitingQueue(groupSize);
            return;
        }

        var table = FindAvailableTable(groupSize);

        if (table == null)
        {
            await SpawnToWaitingQueue(groupSize);
            return;
        }

        await SpawnAndSeatGroup(table, groupSize);
    }

    private DiningTable FindAvailableTable(int groupSize)
    {
        return _levelPresenter.DiningTables
            .FirstOrDefault(t => t.IsEmptyTable() && t.MaxSeats >= groupSize);
    }

    private List<string> GetUniqueCharacterKeys(int groupSize)
    {
        var availableKeys = new List<string>
        {
            PrefabKeys.CustomerCharacter1,
            PrefabKeys.CustomerCharacter2,
            PrefabKeys.CustomerCharacter3,
            PrefabKeys.CustomerCharacter4
        };

        for (int i = availableKeys.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (availableKeys[i], availableKeys[j]) = (availableKeys[j], availableKeys[i]);
        }

        return availableKeys.Take(groupSize).ToList();
    }

    private async UniTask SpawnToWaitingQueue(int groupSize)
    {
        var group = new CustomerGroup(groupSize);
        int currentGroupIndex = _waitingList.Count;
        var characterKeys = GetUniqueCharacterKeys(groupSize);

        for (int i = 0; i < groupSize; i++)
        {
            var customer = await _spawnFactory.Create<CustomerCharacter>(
                PrefabKeys.GetPrefabPath(characterKeys[i]));
            customer.transform.SetParent(_levelPresenter.LevelTransform);
            customer.SetSpawnPosition(_levelPresenter.SpawnPosition);
            customer.WarpTo(_levelPresenter.SpawnPosition);

            group.AddMember(customer);

            Vector3 waitPosition = CalculateWaitingPosition(currentGroupIndex, i);
            customer.EnqueueGoToWaitingPosition(waitPosition, _levelPresenter.WaitingRotation);
        }

        _waitingList.Add(group);
    }

    private async UniTask SpawnAndSeatGroup(DiningTable table, int groupSize)
    {
        var characterKeys = GetUniqueCharacterKeys(groupSize);

        for (int i = 0; i < groupSize; i++)
        {
            var customer = await _spawnFactory.Create<CustomerCharacter>(
                PrefabKeys.GetPrefabPath(characterKeys[i]));
            customer.transform.SetParent(_levelPresenter.LevelTransform);
            customer.SetSpawnPosition(_levelPresenter.SpawnPosition);
            customer.WarpTo(_levelPresenter.SpawnPosition);

            customer.EnqueueGoToSeat(table, i);
        }
    }

    private Vector3 CalculateWaitingPosition(int groupIndex, int memberIndex)
    {
        Vector3 basePosition = _levelPresenter.WaitingPosition;
        Vector3 forward = new Vector3(1, 0, 0);

        int totalPeopleAhead = 0;
        for (int i = 0; i < groupIndex; i++)
        {
            totalPeopleAhead += _waitingList[i].GroupSize;
        }
        totalPeopleAhead += memberIndex;

        float offset = totalPeopleAhead * Constants.kWaitingLineSpacing;
        return basePosition + forward * offset;
    }

    public void OnTableAvailable(DiningTable table)
    {
        if (_waitingList.Count == 0) return;

        var firstGroup = _waitingList[0];

        if (!table.IsEmptyTable() || table.MaxSeats < firstGroup.GroupSize)
        {
            return;
        }

        Debug.Log($"[CustomerService] Seating waiting group ({firstGroup.GroupSize} customers) at available table");

        for (int i = 0; i < firstGroup.Members.Count; i++)
        {
            var customer = firstGroup.Members[i];
            customer.EnqueueGoToSeat(table, i);
        }

        _waitingList.RemoveAt(0);
        ReorganizeWaitingLine();
    }

    private void ReorganizeWaitingLine()
    {
        for (int groupIndex = 0; groupIndex < _waitingList.Count; groupIndex++)
        {
            var group = _waitingList[groupIndex];
            for (int memberIndex = 0; memberIndex < group.Members.Count; memberIndex++)
            {
                var customer = group.Members[memberIndex];
                Vector3 newPosition = CalculateWaitingPosition(groupIndex, memberIndex);
                customer.EnqueueGoToWaitingPosition(newPosition, _levelPresenter.WaitingRotation);
            }
        }
    }

}
