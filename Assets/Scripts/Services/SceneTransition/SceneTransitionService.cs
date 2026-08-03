using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionService : ISceneTransitionService
{
    private const string LoadingScene = "LoadingScene";
    private const float MinLoadingDuration = 1f;

    private UniTaskCompletionSource _readySignal;
    private LoadingView _loadingView;

    public bool IsTransitioning { get; private set; }

    public void SetLoadingView(LoadingView view)
    {
        _loadingView = view;
    }

    public void ClearLoadingView()
    {
        _loadingView = null;
    }

    public async UniTask TransitionAsync(string unloadScene, string loadScene)
    {
        if (IsTransitioning)
            return;

        IsTransitioning = true;
        _readySignal = new UniTaskCompletionSource();

        try
        {
            float startTime = Time.realtimeSinceStartup;

            await SceneManager.LoadSceneAsync(LoadingScene, LoadSceneMode.Additive).ToUniTask();

            if (_loadingView != null)
                await _loadingView.FadeIn();

            await SceneManager.UnloadSceneAsync(unloadScene).ToUniTask();

            await SceneManager.LoadSceneAsync(loadScene, LoadSceneMode.Additive).ToUniTask();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(loadScene));

            await _readySignal.Task;

            float elapsed = Time.realtimeSinceStartup - startTime;
            float remaining = MinLoadingDuration - elapsed;
            if (remaining > 0f)
                await UniTask.Delay((int)(remaining * 1000), ignoreTimeScale: true);

            if (_loadingView != null)
                await _loadingView.FadeOut();

            await SceneManager.UnloadSceneAsync(LoadingScene).ToUniTask();
        }
        finally
        {
            IsTransitioning = false;
            _readySignal = null;
        }
    }

    public void NotifySceneReady()
    {
        _readySignal?.TrySetResult();
    }
}
