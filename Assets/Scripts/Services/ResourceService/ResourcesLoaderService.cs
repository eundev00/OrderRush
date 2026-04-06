using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ResourcesLoaderService : IResourcesLoaderService
{
    private readonly Dictionary<string, Object> _cache = new();

    public async UniTask<T> LoadAsync<T>(string key) where T : Object
    {
        if (_cache.TryGetValue(key, out var cached))
            return (T)cached;

        var handle = Addressables.LoadAssetAsync<T>(key);
        var asset = await handle.Task;
        _cache[key] = asset;
        return asset;
    }

    public T LoadSync<T>(string key) where T : Object
    {
        if (_cache.TryGetValue(key, out var cached))
            return (T)cached;

        var handle = Addressables.LoadAssetAsync<T>(key);
        var asset = handle.WaitForCompletion();
        _cache[key] = asset;
        return asset;
    }

    public void Release(string key)
    {
        if (_cache.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _cache.Remove(key);
        }
    }

    public void ReleaseAll()
    {
        foreach (var asset in _cache.Values)
            Addressables.Release(asset);
        _cache.Clear();
    }
}
