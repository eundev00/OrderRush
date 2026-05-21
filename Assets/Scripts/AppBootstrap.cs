using System.Threading;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class AppBootstrap : IAsyncStartable
{
    private readonly IGameDataService _gameDataService;
    private readonly IAccountService _accountService;

    public AppBootstrap(IGameDataService gameDataService, IAccountService accountService)
    {
        _gameDataService = gameDataService;
        _accountService = accountService;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        await _gameDataService.Initialize();
        _accountService.Initialize();

        await SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive)
            .ToUniTask(cancellationToken: cancellation);
    }
}