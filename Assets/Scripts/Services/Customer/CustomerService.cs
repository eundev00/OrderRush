using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Services;
using UniRx;
using UnityEngine;

public class CustomerService : ICustomerService
{
    private readonly ILevelContextPresenter _levelPresenter;
    private float _spawnInterval;
    private readonly SpawnFactory _spawnFactory;
    private readonly IDayProgressService _dayProgressService;
    private int _nextSpawnIndex;
    private int _maxCustomers;
    private int _maxGroupSize;
    private float _lastCheckedElapsed;

    private readonly List<CustomerGroup> _waitingList = new();
    private readonly CompositeDisposable _disposables = new();



    public CustomerService(
        ILevelContextPresenter levelPresenter,
        SpawnFactory spawnFactory,
        IDayProgressService dayProgressService)
    {
        _levelPresenter = levelPresenter;
        _spawnFactory = spawnFactory;
        _dayProgressService = dayProgressService;
    }

    public void Initialize()
    {
        var currentDay = _dayProgressService.CurrentDayContext;
        var daysData = _dayProgressService.CurrentDaysData;

        int dayNumber = currentDay.DayNumber;
        float timeBarDuration = daysData.GetTimeBarDuration(dayNumber);
        _maxCustomers = daysData.GetMaxCustomers(dayNumber);

        _spawnInterval = timeBarDuration / _maxCustomers;
        _nextSpawnIndex = 0;
        _maxGroupSize = 2;
        _lastCheckedElapsed = -1f;

        currentDay.TimeBarElapsed
            .Subscribe(CheckAndSpawn)
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        _waitingList.Clear();
    }

    private async void CheckAndSpawn(float elapsed)
    {
        if (_nextSpawnIndex >= _maxCustomers) return;

        float nextSpawnTime = _nextSpawnIndex * _spawnInterval;

        if (_lastCheckedElapsed < nextSpawnTime && elapsed >= nextSpawnTime)
        {
            _lastCheckedElapsed = elapsed;
            await TrySpawn();
            _nextSpawnIndex++;
        }
    }

    private async UniTask TrySpawn()
    {
        int groupSize = UnityEngine.Random.Range(1, _maxGroupSize + 1);

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

}
