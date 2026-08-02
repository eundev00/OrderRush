using System;
using MessagePipe;
using UniRx;

public class DayEventService : IDayEventService, IDisposable
{
    private readonly IDayProgressService _dayProgressService;
    private readonly IPublisher<RainEvent> _rainPublisher;
    private readonly CompositeDisposable _disposable = new();

    public DayEventService(
        IDayProgressService dayProgressService,
        IPublisher<RainEvent> rainPublisher,
        ISubscriber<GameCleanupEvent> cleanup)
    {
        _dayProgressService = dayProgressService;
        _rainPublisher = rainPublisher;

        cleanup.Subscribe(_ => Apply(_dayProgressService.CurrentDayContext.DayNumber.Value))
            .AddTo(_disposable);
    }

    public void Initialize(int dayNumber)
    {
        Apply(dayNumber);
    }

    private void Apply(int dayNumber)
    {
        var evt = _dayProgressService.CurrentDaysData?.GetStoryForDay(dayNumber)?.Event;
        _rainPublisher.Publish(new RainEvent(evt != null && evt.IsRainy));
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}
