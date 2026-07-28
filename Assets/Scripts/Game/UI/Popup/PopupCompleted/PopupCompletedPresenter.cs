using UniRx;

public class PopupCompletedPresenter : PopupPresenterBase<int, DayCompletedAction>
{
    private readonly PopupCompleted _view;
    private readonly ISoundService _soundService;

    public PopupCompletedPresenter(
        PopupCompleted view,
        ISoundService soundService) : base(view)
    {
        _view = view;
        _soundService = soundService;
    }

    protected override void OnBind()
    {
        _view.NextButton.onClick.AddListener(OnNextButtonClicked);
        _view.ExitButton.onClick.AddListener(OnExitButtonClicked);

        Disposables.Add(Disposable.Create(() =>
        {
            _view.NextButton.onClick.RemoveListener(OnNextButtonClicked);
            _view.ExitButton.onClick.RemoveListener(OnExitButtonClicked);
        }));
    }

    protected override void OnShow(int earnedCoins)
    {
        _view.SetEarnedCoins(earnedCoins);
    }

    private void OnNextButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close(DayCompletedAction.Next);
    }

    private void OnExitButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close(DayCompletedAction.Exit);
    }
}
