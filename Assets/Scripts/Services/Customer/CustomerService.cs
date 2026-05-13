using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

public class CustomerService : ICustomerService, ITickable
{
    private LevelContext _levelContext;
    private float _spawnInterval;
    private readonly SpawnFactory _spawnFactory;
    private readonly ILevelProgressService _levelProgressService;
    private float _timer;
    private int _spawnCount;
    private int _maxGroupSize;

    private readonly List<CustomerGroup> _waitingList = new();
    private readonly ISubscriber<TableAvailableEvent> _tableAvailableSubscriber;
    private IDisposable _subscription;

    private const float SPACING = 1.5f;

    public CustomerService(
        SpawnFactory spawnFactory,
        ILevelProgressService levelProgressService,
        ISubscriber<TableAvailableEvent> tableAvailableSubscriber)
    {
        _spawnFactory = spawnFactory;
        _levelProgressService = levelProgressService;
        _tableAvailableSubscriber = tableAvailableSubscriber;
    }

    public void SetTables(LevelContext levelContext, LevelData levelData)
    {
        _levelContext = levelContext;
        _spawnInterval = levelData.CustomerSpawnInterval;
        _spawnCount = levelData.MaxCustomers;
        _maxGroupSize = levelData.MaxGroupSize;

        // 이벤트 구독
        _subscription?.Dispose();
        _subscription = _tableAvailableSubscriber.Subscribe(OnTableAvailable);
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _waitingList.Clear();
    }

    public async void Tick()
    {
        if (_spawnCount <= 0) return;
        if (_levelContext == null) return;

        _timer += Time.deltaTime;

        if (_timer >= _spawnInterval)
        {
            _timer = 0;
            int count = await TrySpawn();
            _spawnCount -= count;
        }
    }

    private async UniTask<int> TrySpawn()
    {
        int groupSize = UnityEngine.Random.Range(1, _maxGroupSize + 1);

        if (_waitingList.Count > 0)
        {
            await SpawnToWaitingQueue(groupSize);
            return groupSize;
        }

        var table = FindAvailableTable(groupSize);

        if (table == null)
        {
            await SpawnToWaitingQueue(groupSize);
            return groupSize;
        }

        await SpawnAndSeatGroup(table, groupSize);
        return groupSize;
    }

    private DiningTable FindAvailableTable(int groupSize)
    {
        return _levelContext.DiningTables
            .FirstOrDefault(t => t.IsEmptyTable() && t.MaxSeats >= groupSize);
    }

    private async UniTask SpawnToWaitingQueue(int groupSize)
    {
        var group = new CustomerGroup(groupSize);
        int currentGroupIndex = _waitingList.Count;

        for (int i = 0; i < groupSize; i++)
        {
            var customer = await _spawnFactory.Create<CustomerCharacter>(
                PrefabKeys.GetPrefabPath(PrefabKeys.CustomerCharacter1));
            customer.transform.SetParent(_levelContext.transform);
            customer.SetSpawnPosition(_levelContext.SpawnPoint.position);
            customer.WarpTo(_levelContext.SpawnPoint.position);

            group.AddMember(customer);

            Vector3 waitPosition = CalculateWaitingPosition(currentGroupIndex, i);
            customer.EnqueueGoToWaitingPosition(waitPosition, _levelContext.WaitingPoint.rotation);
        }

        _waitingList.Add(group);
    }

    private async UniTask SpawnAndSeatGroup(DiningTable table, int groupSize)
    {
        for (int i = 0; i < groupSize; i++)
        {
            var customer = await _spawnFactory.Create<CustomerCharacter>(
                PrefabKeys.GetPrefabPath(PrefabKeys.CustomerCharacter1));
            customer.transform.SetParent(_levelContext.transform);
            customer.SetSpawnPosition(_levelContext.SpawnPoint.position);
            customer.WarpTo(_levelContext.SpawnPoint.position);
            customer.EnqueueGoToSeat(table, i);
        }
    }

    private Vector3 CalculateWaitingPosition(int groupIndex, int memberIndex)
    {
        Vector3 basePosition = _levelContext.WaitingPoint.position;
        Vector3 forward = new Vector3(1, 0, 0);

        int totalPeopleAhead = 0;
        for (int i = 0; i < groupIndex; i++)
        {
            totalPeopleAhead += _waitingList[i].GroupSize;
        }
        totalPeopleAhead += memberIndex;

        float offset = totalPeopleAhead * SPACING;
        return basePosition + forward * offset;
    }

    private void OnTableAvailable(TableAvailableEvent evt)
    {
        if (_waitingList.Count > 0)
        {
            TryAssignWaitingGroupToTable(evt.Table);
        }
    }

    private void TryAssignWaitingGroupToTable(DiningTable table)
    {
        if (_waitingList.Count == 0) return;

        var targetGroup = _waitingList.FirstOrDefault(g => g.GroupSize <= table.MaxSeats);

        if (targetGroup == null) return;

        _waitingList.Remove(targetGroup);

        targetGroup.AssignedTable = table;
        for (int i = 0; i < targetGroup.Members.Count; i++)
        {
            var customer = targetGroup.Members[i];
            customer.EnqueueGoToSeat(table, i);
        }

        RepositionWaitingQueue();
    }

    private void RepositionWaitingQueue()
    {
        int groupIndex = 0;
        foreach (var group in _waitingList)
        {
            for (int memberIndex = 0; memberIndex < group.Members.Count; memberIndex++)
            {
                var customer = group.Members[memberIndex];
                Vector3 newPosition = CalculateWaitingPosition(groupIndex, memberIndex);

                customer.EnqueueMoveToWaitingPosition(newPosition, _levelContext.WaitingPoint.rotation);
            }
            groupIndex++;
        }
    }

}
