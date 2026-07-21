using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine.SceneManagement;

public class PopupCompletedPresenter : PopupPresenterBase<int>
{
    private readonly PopupCompleted _view;
    private readonly IPopupService _popupService;

    public PopupCompletedPresenter(
        PopupCompleted view,
        IPopupService popupService) : base(view)
    {
        _view = view;
        _popupService = popupService;
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
        _popupService.Open<PopupCardShopPresenter>(PrefabKeys.PopupCardShop).Forget();
        Close();
    }

    private void OnExitButtonClicked()
    {
        Close();
        SceneManager.UnloadSceneAsync("GameplayScene");
        SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    }
}
