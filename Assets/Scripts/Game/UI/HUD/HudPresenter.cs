using System;
using OrderRush.Services;
using UniRx;
using VContainer.Unity;

public class HudPresenter : IStartable, IDisposable
{
    readonly IDayProgressService _dayProgressService;
    readonly HudView _hudView;
    readonly CompositeDisposable _disposable = new();

    public HudPresenter(IDayProgressService dayProgressService, HudView hudView)
    {
        _dayProgressService = dayProgressService;
        _hudView = hudView;
    }

    public void Start()
    {
        var dayContext = _dayProgressService.CurrentDayContext;

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
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

}
