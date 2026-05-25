using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

public abstract class BaseUIFactory<TView, TPresenter>
    where TView : Component, IUIView
    where TPresenter : BaseUIPresenter<TView>
{
    protected readonly Camera _camera;
    protected readonly RectTransform _canvasRectTransform;

    protected readonly ObjectPool<TView> _viewPool;
    protected readonly GameObject _viewPrefab;
    protected const int MaxPoolSize = 20;

    private readonly List<TPresenter> _activePresenters = new();

    protected BaseUIFactory(RectTransform canvasRectTransform, string prefabKey)
    {
        _canvasRectTransform = canvasRectTransform;
        _camera = Camera.main;

        var handle = Addressables.LoadAssetAsync<GameObject>(PrefabKeys.GetPrefabPath(prefabKey));
        _viewPrefab = handle.WaitForCompletion();

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
        view.Show();
    }

    protected virtual void OnReleaseView(TView view)
    {
        view.Hide();
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
        var view = _viewPool.Get();
        return view;
    }

    public TPresenter Create(Transform target, Vector3 offset)
    {
        var presenter = CreatePresenter(target, offset);
        _activePresenters.Add(presenter);
        return presenter;
    }
    protected abstract TPresenter CreatePresenter(Transform target, Vector3 offset);

    public void Release(TPresenter presenter)
    {
        if (presenter == null) return;

        var view = presenter.View;
        presenter.Dispose();

        if (view != null)
        {
            _activePresenters.Remove(presenter);
            _viewPool.Release(view);
        }
    }

    public void ReleaseAll()
    {
        foreach (var presenter in _activePresenters)
            Release(presenter);
    }
}
