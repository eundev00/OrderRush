using System;
using Cysharp.Threading.Tasks;
using UniRx;
using VContainer.Unity;

public class LobbyPresenter : IStartable, IDisposable
{
    private readonly LobbyView _view;
    private readonly IAccountService _accountService;
    private readonly ISoundService _soundService;
    private readonly ISceneTransitionService _sceneTransition;
    private readonly CompositeDisposable _disposable = new();

    public LobbyPresenter(
        LobbyView view,
        IAccountService accountService,
        ISoundService soundService,
        ISceneTransitionService sceneTransition)
    {
        _view = view;
        _accountService = accountService;
        _soundService = soundService;
        _sceneTransition = sceneTransition;
    }

    public void Start()
    {
        _soundService.PlayBgm(AudioKeys.Bgm1);
        _sceneTransition.NotifySceneReady();

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
        _sceneTransition.TransitionAsync("LobbyScene", "GameplayScene").Forget();
    }

    private void OnContinueButtonClicked()
    {
        _sceneTransition.TransitionAsync("LobbyScene", "GameplayScene").Forget();
    }

    public void Dispose() => _disposable.Dispose();
}
