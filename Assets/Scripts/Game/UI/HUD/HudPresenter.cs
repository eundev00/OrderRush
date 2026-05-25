using System;
using MessagePipe;
using OrderRush.Services;
using UniRx;
using VContainer.Unity;

public class HudPresenter : IStartable, IDisposable
{
    readonly IDayProgressService _dayProgressService;
    readonly ISubscriber<GameCleanupEvent> _gameCleanupSubscriber;
    readonly HudView _hudView;
    readonly CompositeDisposable _disposable = new();

    public HudPresenter(
        IDayProgressService dayProgressService,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber,
        HudView hudView)
    {
        _dayProgressService = dayProgressService;
        _gameCleanupSubscriber = gameCleanupSubscriber;
        _hudView = hudView;
    }

    public void Start()
    {
        var dayContext = _dayProgressService.CurrentDayContext;

        UpdateDayInfo();

        dayContext.TimeBarElapsed
            .Subscribe(elapsed =>
            {
                float remainingRatio = 1f - (elapsed / dayContext.TimeBarDuration);
                _hudView.SetTimeGauge(remainingRatio);
            })
            .AddTo(_disposable);

        dayContext.EarnedCoins
            .Subscribe(coins => _hudView.SetCoin(coins))
            .AddTo(_disposable);

        _gameCleanupSubscriber
            .Subscribe(_ => UpdateDayInfo())
            .AddTo(_disposable);
    }

    private void UpdateDayInfo()
    {
        var dayContext = _dayProgressService.CurrentDayContext;
        var daysData = _dayProgressService.CurrentDaysData;

        int dayNumber = dayContext.DayNumber;
        int maxCustomers = daysData.GetMaxCustomers(dayNumber);

        _hudView.SetDay(dayNumber);
        _hudView.SetMaxCustomers(maxCustomers);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

}
