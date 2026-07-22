using System.Threading;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class AppBootstrap : IAsyncStartable
{
    private readonly IGameDataService _gameDataService;
    private readonly IAccountService _accountService;
    private readonly IPopupService _popupService;
    private readonly ISoundService _soundService;
    private readonly IObjectResolver _resolver; // Root 리졸버 — 공통 팝업의 부모

    public AppBootstrap(
        IGameDataService gameDataService,
        IAccountService accountService,
        IPopupService popupService,
        ISoundService soundService,
        IObjectResolver resolver)
    {
        _gameDataService = gameDataService;
        _accountService = accountService;
        _popupService = popupService;
        _soundService = soundService;
        _resolver = resolver;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        await _gameDataService.Initialize();
        await _soundService.Initialize();
        _accountService.Initialize();

        await _popupService.Initialize();
        RegisterCommonPopups();

        await SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive)
            .ToUniTask(cancellationToken: cancellation);
    }

    // 공통 팝업: 부모 = Root 리졸버 → 어느 씬이 열든 씬 전환에도 살아남는다(로딩 등).
    private void RegisterCommonPopups()
    {
        _popupService.RegisterPopup(PrefabKeys.MessagePopup, _resolver);
        // TODO: Loading/Alert/Confirm 등 공통 팝업 추가 시 여기에 등록.
    }
}
