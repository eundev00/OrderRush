using System;
using OrderRush.Data;
using OrderRush.Services;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class LobbyPresenter : IStartable, IDisposable
{
    private readonly LobbyView _view;
    private readonly IResourcesLoaderService _resourcesLoaderService;
    private readonly IAccountService _accountService;
    private readonly CompositeDisposable _disposable = new();

    public LobbyPresenter(
        LobbyView view,
        IResourcesLoaderService resourcesLoaderService,
        IAccountService accountService)
    {
        _view = view;
        _resourcesLoaderService = resourcesLoaderService;
        _accountService = accountService;
    }


    public void Start()
    {
        _view.SetDay(_accountService.Account.CurrentDay);

        _view.StartButton
            .OnClickAsObservable()
            .Subscribe(_ => OnStartButtonClicked())
            .AddTo(_disposable);
    }

    private void OnStartButtonClicked()
    {
        SceneManager.UnloadSceneAsync("LobbyScene");
        SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);
    }

    public void Dispose() => _disposable.Dispose();
}