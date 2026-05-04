using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;

public abstract class BaseUIFactory<TView, TPresenter>
    where TView : Component, IUIView
    where TPresenter : BaseUIPresenter<TView>
{
    protected readonly Camera _camera;
    protected readonly RectTransform _canvasRectTransform;

    protected readonly ObjectPool<TView> _viewPool;
    protected readonly GameObject _viewPrefab;
    protected const int MaxPoolSize = 20;

    protected BaseUIFactory(RectTransform canvasRectTransform, string prefabKey)
    {
        _canvasRectTransform = canvasRectTransform;
        _camera = Camera.main;

        // 어드레서블에서 프리팹 로드 (동기)
        var handle = Addressables.LoadAssetAsync<GameObject>(PrefabKeys.GetPrefabPath(prefabKey));
        _viewPrefab = handle.WaitForCompletion();

        // Unity ObjectPool 생성
        _viewPool = new ObjectPool<TView>(
            createFunc: CreateView,
            actionOnGet: OnGetView,
            actionOnRelease: OnReleaseView,
            actionOnDestroy: OnDestroyView,
            defaultCapacity: 5,
            maxSize: MaxPoolSize
        );
    }

    private TView CreateView()
    {
        var go = UnityEngine.Object.Instantiate(_viewPrefab, _canvasRectTransform);
        return go.GetComponent<TView>();
    }

    protected virtual void OnGetView(TView view)
    {
        view.gameObject.SetActive(true);
    }

    protected virtual void OnReleaseView(TView view)
    {
        view.gameObject.SetActive(false);
    }

    private void OnDestroyView(TView view)
    {
        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    protected TView GetViewFromPool()
    {
        return _viewPool.Get();
    }

    public abstract TPresenter Create(
        Transform target,
        Vector3 offset,
        Sprite icon = null);

    public void Release(TPresenter presenter)
    {
        if (presenter == null) return;

        var view = presenter.View;

        // 1. Presenter 정리 (구독 정리 등)
        presenter.Dispose();

        // 2. View를 풀에 반환
        if (view != null)
        {
            _viewPool.Release(view);
        }
    }
}
