using UniRx;

public class PopupStoryPresenter : PopupPresenterBase<StoryDayData>
{
    private readonly PopupStoryView _view;
    private readonly ISoundService _soundService;

    public PopupStoryPresenter(PopupStoryView view, ISoundService soundService) : base(view)
    {
        _view = view;
        _soundService = soundService;
    }

    protected override void OnBind()
    {
        _view.ContinueButton.onClick.AddListener(OnContinue);
        Disposables.Add(Disposable.Create(
            () => _view.ContinueButton.onClick.RemoveListener(OnContinue)));
    }

    protected override void OnShow(StoryDayData day)
    {
        _view.SetDayNumber(day.DayNumber);
        _view.SetText(day.StoryText);
    }

    private void OnContinue()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close();
    }
}
