using UniRx;

public class PopupFailedPresenter : PopupPresenterBaseNoArgs<DayFailedAction>
{
    private readonly PopupDayFailed _view;
    private readonly ISoundService _soundService;

    public PopupFailedPresenter(
        PopupDayFailed view,
        ISoundService soundService) : base(view)
    {
        _view = view;
        _soundService = soundService;
    }

    protected override void OnBind()
    {
        _view.RestartButton.onClick.AddListener(OnRestartButtonClicked);
        _view.ExitButton.onClick.AddListener(OnExitButtonClicked);

        Disposables.Add(Disposable.Create(() =>
        {
            _view.RestartButton.onClick.RemoveListener(OnRestartButtonClicked);
            _view.ExitButton.onClick.RemoveListener(OnExitButtonClicked);
        }));
    }

    private void OnRestartButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close(DayFailedAction.Restart);
    }

    private void OnExitButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close(DayFailedAction.Exit);
    }
}
