using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Services;
using UniRx;
using VContainer.Unity;

public class GameUIContextPresenter : IStartable, IDisposable
{
    private readonly GameUIContext _gameUIContext;
    private readonly IDayProgressService _dayProgressService;
    private readonly ISubscriber<DayEndedEvent> _dayEndedSubscriber;
    private readonly IPopupService _popupService;
    private readonly CompositeDisposable _disposable = new();

    private PopupCompletedPresenter _popupCompletedPresenter;
    private PopupFailedPresenter _popupFailedPresenter;

    public GameUIContextPresenter(
        GameUIContext gameUIContext,
        IDayProgressService dayProgressService,
        ISubscriber<DayEndedEvent> dayEndedSubscriber,
        IPopupService popupService)
    {
        _gameUIContext = gameUIContext;
        _dayProgressService = dayProgressService;
        _dayEndedSubscriber = dayEndedSubscriber;
        _popupService = popupService;
    }

    public void Start()
    {
        _popupCompletedPresenter = new PopupCompletedPresenter(_gameUIContext.PopupCompleted, _dayProgressService, ShowCardShop);
        _popupFailedPresenter = new PopupFailedPresenter(_gameUIContext.PopupDayFailed, _dayProgressService);

        _popupCompletedPresenter.Start();
        _popupFailedPresenter.Start();

        _dayEndedSubscriber
            .Subscribe(_ => OnDayEnded())
            .AddTo(_disposable);
    }

    private void OnDayEnded()
    {
        var dayContext = _dayProgressService.CurrentDayContext;

        if (dayContext.IsCompleted)
        {
            _popupCompletedPresenter.ShowPopup();
        }
        else
        {
            _popupFailedPresenter.ShowPopup();
        }
    }

    private async void ShowCardShop()
    {
        await _popupService.Open<PopupCardShopPresenter>(PrefabKeys.PopupCardShop);
    }

    public void Dispose()
    {
        _disposable?.Dispose();
        _popupCompletedPresenter?.Dispose();
        _popupFailedPresenter?.Dispose();
    }
}
