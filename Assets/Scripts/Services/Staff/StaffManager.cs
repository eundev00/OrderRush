using System;
using System.Collections.Generic;
using MessagePipe;

public class StaffManager
{
    private readonly ISubscriber<OrderNeededEvent> _orderNeededSubscriber;
    private readonly ISubscriber<PlateOnCounterEvent> _plateOnCounterSubscriber;
    private readonly ISubscriber<DirtyPlateEvent> _dirtyPlateSubscriber;
    private readonly ISubscriber<GameCleanupEvent> _gameCleanupSubscriber;
    private readonly ILevelService _levelService;

    private readonly List<WorkItem> _pending = new();
    private IDisposable _disposables;

    public event Action WorkAdded;

    public StaffManager(
        ISubscriber<OrderNeededEvent> orderNeededSubscriber,
        ISubscriber<PlateOnCounterEvent> plateOnCounterSubscriber,
        ISubscriber<DirtyPlateEvent> dirtyPlateSubscriber,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber,
        ILevelService levelService)
    {
        _orderNeededSubscriber = orderNeededSubscriber;
        _plateOnCounterSubscriber = plateOnCounterSubscriber;
        _dirtyPlateSubscriber = dirtyPlateSubscriber;
        _gameCleanupSubscriber = gameCleanupSubscriber;
        _levelService = levelService;
    }

    public void Initialize()
    {
        var bag = DisposableBag.CreateBuilder();
        _orderNeededSubscriber.Subscribe(e => Enqueue(new TakeOrderWorkItem(e.Table))).AddTo(bag);
        _plateOnCounterSubscriber.Subscribe(OnPlateOnCounter).AddTo(bag);
        _dirtyPlateSubscriber.Subscribe(e => Enqueue(new ClearPlateWorkItem(e.Table))).AddTo(bag);
        _gameCleanupSubscriber.Subscribe(_ => OnGameCleanup()).AddTo(bag);
        _disposables = bag.Build();
    }

    public void Enqueue(WorkItem item)
    {
        _pending.Add(item);
        WorkAdded?.Invoke();
    }

    public WorkItem TryDequeue(Func<WorkType, bool> canHandle)
    {
        for (int i = 0; i < _pending.Count;)
        {
            var item = _pending[i];
            if (!canHandle(item.Type))
            {
                i++;
                continue;
            }

            _pending.RemoveAt(i);
            if (item.IsValid())
                return item;
        }
        return null;
    }

    private void OnPlateOnCounter(PlateOnCounterEvent e)
    {
        if (e.Counter.CurrentItem is not Plate plate || plate.MatchedRecipeID == -1)
            return;

        DiningTable earliestTable = null;
        float earliestTime = float.MaxValue;
        foreach (var table in _levelService.Context.DiningTables)
        {
            float orderTime = table.GetEarliestOrderTime(plate.MatchedRecipeID);
            if (orderTime < earliestTime)
            {
                earliestTime = orderTime;
                earliestTable = table;
            }
        }

        if (earliestTable != null)
            Enqueue(new ServeFoodWorkItem(e.Counter, earliestTable));
    }

    private void OnGameCleanup()
    {
        _pending.Clear();
        _disposables?.Dispose();
    }
}
