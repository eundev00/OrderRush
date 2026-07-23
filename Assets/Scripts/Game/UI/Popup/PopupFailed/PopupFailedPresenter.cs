using UniRx;
using UnityEngine.SceneManagement;

public class PopupFailedPresenter : PopupPresenterBase
{
    private readonly PopupDayFailed _view;
    private readonly IDayProgressService _dayProgressService;
    private readonly ISoundService _soundService;

    public PopupFailedPresenter(
        PopupDayFailed view,
        IDayProgressService dayProgressService,
        ISoundService soundService) : base(view)
    {
        _view = view;
        _dayProgressService = dayProgressService;
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
        Close();
        _dayProgressService.RestartDay();
    }

    private void OnExitButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        Close();
        SceneManager.UnloadSceneAsync("GameplayScene");
        SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    }
}
