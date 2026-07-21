using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using VContainer.Unity;

public class GameUIContextPresenter : IStartable, IDisposable
{
    private readonly GameUIContext _uiContext;
    private readonly ISubscriber<DayEndedEvent> _dayEndedSubscriber;
    private readonly IPopupService _popupService;
    private readonly CompositeDisposable _disposable = new();

    public GameUIContextPresenter(
        GameUIContext uiContext,
        ISubscriber<DayEndedEvent> dayEndedSubscriber,
        IPopupService popupService)
    {
        _uiContext = uiContext;
        _dayEndedSubscriber = dayEndedSubscriber;
        _popupService = popupService;
    }

    public void Start()
    {
        _popupService.SetCanvasRoot(_uiContext.PopupRoot);

        _dayEndedSubscriber
            .Subscribe(OnDayEnded)
            .AddTo(_disposable);
    }

    private void OnDayEnded(DayEndedEvent evt)
    {
        if (evt.IsCompleted)
            _popupService.Open<PopupCompletedPresenter, int>(PrefabKeys.PopupCompleted, evt.EarnedCoins).Forget();
        else
            _popupService.Open<PopupFailedPresenter>(PrefabKeys.PopupFailed).Forget();
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}
