using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SpawnFactory : System.IDisposable
{
    private readonly IResourcesLoaderService _resourceLoader;
    private readonly IObjectResolver _container;
    private readonly List<GameObject> _spawnedObjects = new();
    private readonly IDisposable _gameCleanupSubscription;



    public SpawnFactory(
        IResourcesLoaderService resourceLoader,
        IObjectResolver container,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber)
    {
        _resourceLoader = resourceLoader;
        _container = container;
        _gameCleanupSubscription = gameCleanupSubscriber.Subscribe(_ => OnGameCleanup());
    }

    public async UniTask<T> Create<T>(string key) where T : Component
    {
        var prefab = await _resourceLoader.LoadAsync<GameObject>(key);
        var obj = UnityEngine.Object.Instantiate(prefab);
        _container.InjectGameObject(obj);

        var tracker = obj.AddComponent<SpawnedObjectTracker>();
        tracker.Initialize(this);

        var component = obj.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"[SpawnFactory] {key} does not have {typeof(T)}");
            UnityEngine.Object.Destroy(obj);
            return null;
        }

        _spawnedObjects.Add(obj);
        return component;
    }

    public async UniTask<GameObject> Create(string key)
    {
        var prefab = await _resourceLoader.LoadAsync<GameObject>(key);
        var obj = UnityEngine.Object.Instantiate(prefab);
        _container.InjectGameObject(obj);

        var tracker = obj.AddComponent<SpawnedObjectTracker>();
        tracker.Initialize(this);

        _spawnedObjects.Add(obj);
        return obj;
    }

    public void Destroy(GameObject obj)
    {
        _spawnedObjects.Remove(obj);
        UnityEngine.Object.Destroy(obj);
    }

    public void DestroyAll()
    {
        foreach (var obj in _spawnedObjects)
        {
            if (obj != null)
                UnityEngine.Object.Destroy(obj);
        }
        _spawnedObjects.Clear();
    }

    internal void OnObjectDestroyed(GameObject obj)
    {
        _spawnedObjects.Remove(obj);
    }

    private void OnGameCleanup()
    {
        DestroyAll();
    }

    public void Dispose()
    {
        _gameCleanupSubscription?.Dispose();
    }
}