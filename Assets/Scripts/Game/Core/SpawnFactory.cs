using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SpawnFactory
{
    private readonly IResourcesLoaderService _resourceLoader;
    private readonly IObjectResolver _container;

    public SpawnFactory(IResourcesLoaderService resourceLoader, IObjectResolver container)
    {
        _resourceLoader = resourceLoader;
        _container = container;
    }

    public async UniTask<GameObject> Create(string key)
    {
        var prefab = await _resourceLoader.LoadAsync<GameObject>(key);
        var obj = Object.Instantiate(prefab);
        _container.InjectGameObject(obj);
        return obj;
    }

    public async UniTask<T> Create<T>(string key) where T : Component
    {
        var obj = await Create(key);
        return GetRequiredComponent<T>(obj, key);
    }

    public async UniTask<T> Create<T>(string key, Vector3 position, Transform parent) where T : Component
    {
        var prefab = await _resourceLoader.LoadAsync<GameObject>(key);
        var obj = Object.Instantiate(prefab, position, Quaternion.identity, parent);
        _container.InjectGameObject(obj);
        return GetRequiredComponent<T>(obj, key);
    }

    private static T GetRequiredComponent<T>(GameObject obj, string key) where T : Component
    {
        var component = obj.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"[SpawnFactory] {key} does not have {typeof(T)}");
            Object.Destroy(obj);
            return null;
        }
        return component;
    }
}
