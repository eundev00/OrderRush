using OrderRush.Services;
using UniRx;
using UnityEngine.SceneManagement;

// 하루 실패 팝업 Presenter — 코인 미획득 → 다시하기/나가기.
//   다시하기 = 팝업 닫고 하루 재시작.
//   나가기   = 팝업 닫고 로비 씬으로 전환.
public class PopupFailedPresenter : PopupPresenterBase
{
    private readonly PopupDayFailed _view;
    private readonly IDayProgressService _dayProgressService;

    public PopupFailedPresenter(
        PopupDayFailed view,
        IDayProgressService dayProgressService) : base(view)
    {
        _view = view;
        _dayProgressService = dayProgressService;
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
        Close();
        _dayProgressService.RestartDay();
    }

    private void OnExitButtonClicked()
    {
        Close();
        SceneManager.UnloadSceneAsync("GameplayScene");
        SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    }
}
