using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameObjectFactory
{
    readonly IObjectResolver _container;
    readonly IResourcesLoaderService _loader;

    public GameObjectFactory(
        IObjectResolver container,
        IResourcesLoaderService loader)
    {
        _container = container;
        _loader = loader;
    }

    public async UniTask<T> CreateAsync<T>(string key) where T : Component
    {
        var prefab = await _loader.LoadAsync<GameObject>(key);

        var obj = _container.Instantiate(prefab);
        var component = obj.GetComponent<T>();

        if (component == null)
        {
            Debug.LogError($"[Factory] {key} does not have {typeof(T)}");
            Object.Destroy(obj);
            return null;
        }

        return component;
    }

    public async UniTask<GameObject> CreateAsync(string key)
    {
        var prefab = await _loader.LoadAsync<GameObject>(key);
        return _container.Instantiate(prefab);
    }
}