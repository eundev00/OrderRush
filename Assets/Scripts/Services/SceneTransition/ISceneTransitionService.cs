using Cysharp.Threading.Tasks;

public interface ISceneTransitionService
{
    bool IsTransitioning { get; }
    UniTask TransitionAsync(string unloadScene, string loadScene);
    void NotifySceneReady();
    void SetLoadingView(LoadingView view);
    void ClearLoadingView();
}
