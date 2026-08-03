using System;
using VContainer.Unity;

public class LoadingPresenter : IStartable, IDisposable
{
    private readonly ISceneTransitionService _sceneTransition;
    private readonly LoadingView _loadingView;

    public LoadingPresenter(ISceneTransitionService sceneTransition, LoadingView loadingView)
    {
        _sceneTransition = sceneTransition;
        _loadingView = loadingView;
    }

    public void Start()
    {
        _sceneTransition.SetLoadingView(_loadingView);
    }

    public void Dispose()
    {
        _sceneTransition.ClearLoadingView();
    }
}
