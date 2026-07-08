using System;
using MessagePipe;
using OrderRush.Services;
using UniRx;
using VContainer.Unity;

public class HudPresenter : IStartable, IDisposable
{
    readonly IDayProgressService _dayProgressService;
    readonly IAccountService _accountService;
    readonly ISubscriber<GameCleanupEvent> _gameCleanupSubscriber;
    readonly HudView _hudView;
    readonly CompositeDisposable _disposable = new();

    public HudPresenter(
        IDayProgressService dayProgressService,
        IAccountService accountService,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber,
        HudView hudView)
    {
        _dayProgressService = dayProgressService;
        _accountService = accountService;
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

        _accountService.Account.Coins
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
