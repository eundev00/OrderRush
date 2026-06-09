using System;
using MessagePipe;
using OrderRush.Services;
using UniRx;
using VContainer.Unity;

public class GameUIContextPresenter : IStartable, IDisposable
{
    private readonly GameUIContext _gameUIContext;
    private readonly IDayProgressService _dayProgressService;
    private readonly ISubscriber<DayEndedEvent> _dayEndedSubscriber;
    private readonly IShopService _shopService;
    private readonly IAccountService _accountService;
    private readonly CompositeDisposable _disposable = new();

    private PopupCompletedPresenter _popupCompletedPresenter;
    private PopupFailedPresenter _popupFailedPresenter;
    private PopupCardShopPresenter _popupCardShopPresenter;

    public GameUIContextPresenter(
        GameUIContext gameUIContext,
        IDayProgressService dayProgressService,
        ISubscriber<DayEndedEvent> dayEndedSubscriber,
        IShopService cardService,
        IAccountService accountService)
    {
        _gameUIContext = gameUIContext;
        _dayProgressService = dayProgressService;
        _dayEndedSubscriber = dayEndedSubscriber;
        _shopService = cardService;
        _accountService = accountService;
    }

    public void Start()
    {
        _popupCompletedPresenter = new PopupCompletedPresenter(_gameUIContext.PopupCompleted, _dayProgressService);
        _popupFailedPresenter = new PopupFailedPresenter(_gameUIContext.PopupDayFailed, _dayProgressService);
        _popupCardShopPresenter = new PopupCardShopPresenter(_gameUIContext.PopupCardShop, _shopService, _accountService);

        _popupCompletedPresenter.Start();
        _popupFailedPresenter.Start();
        _popupCardShopPresenter.Start();

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

    public void ShowCardShop()
    {
        _popupCardShopPresenter.ShowPopup();
    }

    public void Dispose()
    {
        _disposable?.Dispose();
        _popupCompletedPresenter?.Dispose();
        _popupFailedPresenter?.Dispose();
        _popupCardShopPresenter?.Dispose();
    }
}
