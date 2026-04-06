using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelFactory
{
    private readonly IResourcesLoaderService _resourceLoader;


    public LevelFactory(IResourcesLoaderService resourceLoader)
    {
        _resourceLoader = resourceLoader;
    }

    public async UniTask<LevelContext> CreateLevelContext(string levelPath)
    {

        var levelPrefab = await _resourceLoader.LoadAsync<LevelContext>(levelPath);
        return Object.Instantiate(levelPrefab);
    }

    public void ReleaseLevelContext(string levelPath)
    {
        _resourceLoader.Release(levelPath);
    }
}
