using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine.SceneManagement;

public class PopupCompletedPresenter : PopupPresenterBase<int>
{
    private readonly PopupCompleted _view;
    private readonly IPopupService _popupService;
    private readonly ISoundService _soundService;
    private readonly IStoryFlowService _storyFlow;
    private readonly IDayProgressService _dayProgressService;

    public PopupCompletedPresenter(
        PopupCompleted view,
        IPopupService popupService,
        ISoundService soundService,
        IStoryFlowService storyFlow,
        IDayProgressService dayProgressService) : base(view)
    {
        _view = view;
        _popupService = popupService;
        _soundService = soundService;
        _storyFlow = storyFlow;
        _dayProgressService = dayProgressService;
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

    private async void OnNextButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);

        int enteringDay = _dayProgressService.CurrentDayContext.DayNumber + 1;
        await _storyFlow.ShowStoryForDayAsync(enteringDay);

        _popupService.Open<PopupCardShopPresenter>(PrefabKeys.PopupCardShop).Forget();
        Close();
    }

    private void OnExitButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close();
        SceneManager.UnloadSceneAsync("GameplayScene");
        SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    }
}
