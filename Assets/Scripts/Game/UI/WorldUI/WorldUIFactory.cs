using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

public class WorldUIFactory
{
    private readonly RectTransform _canvasRectTransform;
    private readonly IObjectResolver _container;
    private readonly Dictionary<string, ObjectPool<GameObject>> _pools = new();
    private readonly Dictionary<string, GameObject> _loadedPrefabs = new();
    private const int MaxPoolSize = 20;

    public RectTransform CanvasRectTransform => _canvasRectTransform;

    public WorldUIFactory(RectTransform canvasRectTransform, IObjectResolver container)
    {
        _canvasRectTransform = canvasRectTransform;
        _container = container;
    }

    public GameObject Create(string prefabKey)
    {
        var pool = GetOrCreatePool(prefabKey);
        return pool.Get();
    }

    public void Release(string prefabKey, GameObject obj)
    {
        if (obj == null) return;

        if (_pools.TryGetValue(prefabKey, out var pool))
        {
            pool.Release(obj);
        }
    }

    private ObjectPool<GameObject> GetOrCreatePool(string prefabKey)
    {
        if (_pools.TryGetValue(prefabKey, out var existingPool))
        {
            return existingPool;
        }

        if (!_loadedPrefabs.TryGetValue(prefabKey, out var prefab))
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(PrefabKeys.GetPrefabPath(prefabKey));
            prefab = handle.WaitForCompletion();
            _loadedPrefabs[prefabKey] = prefab;
        }

        var pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => { if (obj != null) Object.Destroy(obj); },
            defaultCapacity: 5,
            maxSize: MaxPoolSize
        );

        _pools[prefabKey] = pool;
        return pool;
    }

    private GameObject CreateObject(GameObject prefab)
    {
        var obj = Object.Instantiate(prefab, _canvasRectTransform);
        _container.InjectGameObject(obj);
        return obj;
    }
}
