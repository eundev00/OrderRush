using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class LobbyPresenter : IStartable, IDisposable
{
    private readonly LobbyView _view;
    private readonly IResourcesLoaderService _resourcesLoaderService;
    private readonly IAccountService _accountService;
    private readonly ISoundService _soundService;
    private readonly CompositeDisposable _disposable = new();

    public LobbyPresenter(
        LobbyView view,
        IResourcesLoaderService resourcesLoaderService,
        IAccountService accountService,
        ISoundService soundService)
    {
        _view = view;
        _resourcesLoaderService = resourcesLoaderService;
        _accountService = accountService;
        _soundService = soundService;
    }


    public void Start()
    {
        _soundService.PlayBgm(AudioKeys.Bgm1);

        _view.SetDay(_accountService.Account.CurrentDay);

        _accountService.Account.Coins
            .Subscribe(coins => _view.SetCoins(coins))
            .AddTo(_disposable);

        _view.NewGameButton
            .OnClickAsObservable()
            .Subscribe(_ => OnNewGameButtonClicked())
            .AddTo(_disposable);

        _view.ContinueButton
            .OnClickAsObservable()
            .Subscribe(_ => OnContinueButtonClicked())
            .AddTo(_disposable);

        _view.CanContinue = CanContinue();
    }

    private bool CanContinue()
    {
        return _accountService.Account.CurrentDay > 1;
    }

    private void OnNewGameButtonClicked()
    {
        _accountService.ResetAll();
        _view.SetDay(_accountService.Account.CurrentDay);
        SceneManager.UnloadSceneAsync("LobbyScene");
        SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);
    }

    private void OnContinueButtonClicked()
    {
        SceneManager.UnloadSceneAsync("LobbyScene");
        SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);
    }

    public void Dispose() => _disposable.Dispose();
}