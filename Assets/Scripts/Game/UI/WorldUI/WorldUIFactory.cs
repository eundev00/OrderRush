using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;

public class WorldUIFactory
{
    private readonly Camera _camera;
    private readonly RectTransform _canvasRectTransform;
    private readonly Dictionary<string, object> _pools = new();
    private readonly Dictionary<string, GameObject> _loadedPrefabs = new();
    private const int MaxPoolSize = 20;

    public WorldUIFactory(RectTransform canvasRectTransform)
    {
        _canvasRectTransform = canvasRectTransform;
        _camera = Camera.main;
    }

    public TView Create<TView>(string prefabKey, Transform target = null, Vector3 offset = default)
        where TView : Component, IUIView
    {
        var pool = GetOrCreatePool<TView>(prefabKey);
        var view = pool.Get();

        if (target != null)
        {
            var tracker = view.GetComponent<WorldUITracker>();
            if (tracker == null)
            {
                tracker = view.gameObject.AddComponent<WorldUITracker>();
            }
            tracker.Initialize(_camera, _canvasRectTransform, target, offset);
        }

        return view;
    }

    public void Release<TView>(string prefabKey, TView view)
        where TView : Component, IUIView
    {
        if (view == null) return;

        if (_pools.TryGetValue(prefabKey, out var poolObj))
        {
            var pool = (ObjectPool<TView>)poolObj;
            pool.Release(view);
        }
    }

    private ObjectPool<TView> GetOrCreatePool<TView>(string prefabKey)
        where TView : Component, IUIView
    {
        if (_pools.TryGetValue(prefabKey, out var existingPool))
        {
            return (ObjectPool<TView>)existingPool;
        }

        if (!_loadedPrefabs.TryGetValue(prefabKey, out var prefab))
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(PrefabKeys.GetPrefabPath(prefabKey));
            prefab = handle.WaitForCompletion();
            _loadedPrefabs[prefabKey] = prefab;
        }

        var pool = new ObjectPool<TView>(
            createFunc: () => CreateView<TView>(prefab),
            actionOnGet: OnGetView,
            actionOnRelease: OnReleaseView,
            actionOnDestroy: OnDestroyView,
            defaultCapacity: 5,
            maxSize: MaxPoolSize
        );

        _pools[prefabKey] = pool;
        return pool;
    }

    private TView CreateView<TView>(GameObject prefab)
        where TView : Component, IUIView
    {
        var go = UnityEngine.Object.Instantiate(prefab, _canvasRectTransform);
        return go.GetComponent<TView>();
    }

    private void OnGetView<TView>(TView view)
        where TView : Component, IUIView
    {
        view.Show();
    }

    private void OnReleaseView<TView>(TView view)
        where TView : Component, IUIView
    {
        view.Hide();
    }

    private void OnDestroyView<TView>(TView view)
        where TView : Component, IUIView
    {
        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }
}
