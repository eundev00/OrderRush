using System;
using MessagePipe;
using UniRx;

public class DayEventService : IDayEventService, IDisposable
{
    private readonly IDayProgressService _dayProgressService;
    private readonly ILevelContextPresenter _levelPresenter;
    private readonly CompositeDisposable _disposable = new();

    public DayEventService(
        IDayProgressService dayProgressService,
        ILevelContextPresenter levelPresenter,
        ISubscriber<GameCleanupEvent> cleanup)
    {
        _dayProgressService = dayProgressService;
        _levelPresenter = levelPresenter;

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
        _levelPresenter.SetRain(evt != null && evt.IsRainy);
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}
