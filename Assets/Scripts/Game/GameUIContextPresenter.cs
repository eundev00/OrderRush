using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Services;
using UniRx;
using VContainer.Unity;

// 게임 씬 UI 코디네이터 — 하루 종료 시 결과 팝업(완료/실패)을 연다.
// 팝업 스코프는 카탈로그(ScenePopupRegistrar 가 게임 리졸버로 등록)가 결정하므로
// 여기서는 키만 넘겨 Open 한다. 완료→카드샵 체이닝은 완료 팝업 Presenter 가 직접 처리.
public class GameUIContextPresenter : IStartable, IDisposable
{
    private readonly GameUIContext _uiContext;
    private readonly IDayProgressService _dayProgressService;
    private readonly ISubscriber<DayEndedEvent> _dayEndedSubscriber;
    private readonly IPopupService _popupService;
    private readonly CompositeDisposable _disposable = new();

    public GameUIContextPresenter(
        GameUIContext uiContext,
        IDayProgressService dayProgressService,
        ISubscriber<DayEndedEvent> dayEndedSubscriber,
        IPopupService popupService)
    {
        _uiContext = uiContext;
        _dayProgressService = dayProgressService;
        _dayEndedSubscriber = dayEndedSubscriber;
        _popupService = popupService;
    }

    public void Start()
    {
        _popupService.SetCanvasRoot(_uiContext.PopupRoot);

        _dayEndedSubscriber
            .Subscribe(_ => OnDayEnded())
            .AddTo(_disposable);
    }

    private void OnDayEnded()
    {
        var dayContext = _dayProgressService.CurrentDayContext;

        if (dayContext.IsCompleted)
            _popupService.Open<PopupCompletedPresenter>(PrefabKeys.PopupCompleted).Forget();
        else
            _popupService.Open<PopupFailedPresenter>(PrefabKeys.PopupFailed).Forget();
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}
