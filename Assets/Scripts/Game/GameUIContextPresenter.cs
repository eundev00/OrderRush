using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;

public class GameUIContextPresenter : IDisposable
{
    private readonly GameUIContext _uiContext;
    private readonly ISubscriber<DayEndedEvent> _dayEndedSubscriber;
    private readonly IPopupService _popupService;
    private readonly IDayFlowService _dayFlowService;
    private readonly CompositeDisposable _disposable = new();

    public GameUIContextPresenter(
        GameUIContext uiContext,
        ISubscriber<DayEndedEvent> dayEndedSubscriber,
        IPopupService popupService,
        IDayFlowService dayFlowService)
    {
        _uiContext = uiContext;
        _dayEndedSubscriber = dayEndedSubscriber;
        _popupService = popupService;
        _dayFlowService = dayFlowService;
    }

    public void Initialize()
    {
        _popupService.SetCanvasRoot(_uiContext.PopupRoot);

        _dayEndedSubscriber
            .Subscribe(OnDayEnded)
            .AddTo(_disposable);
    }

    private void OnDayEnded(DayEndedEvent evt)
    {
        _dayFlowService.HandleDayEndedAsync(evt).Forget();
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}
