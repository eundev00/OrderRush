using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelFactory
{
    private readonly Transform _root;
    private readonly IResourcesLoaderService _resourceLoader;
    private readonly IObjectResolver _container;

    private const string DefaultPath = "Assets/Prefabs/Game/Level/LevelMap{0}.prefab";


    public LevelFactory(IResourcesLoaderService resourceLoader,
                        IObjectResolver container,
                        Transform root)
    {
        _resourceLoader = resourceLoader;
        _container = container;
        _root = root;
    }

    public async UniTask<LevelContext> CreateLevelContext(int level)
    {
        string levelPath = string.Format(DefaultPath, level);
        var levelPrefab = await _resourceLoader.LoadAsync<GameObject>(levelPath);
        var levelInstance = Object.Instantiate(levelPrefab, _root);
        _container.InjectGameObject(levelInstance);
        return levelInstance.GetComponent<LevelContext>();
    }

    public void ReleaseLevelContext(int level)
    {
        string levelPath = string.Format(DefaultPath, level);
        _resourceLoader.Release(levelPath);
    }
}
