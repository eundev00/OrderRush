using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelsDataService : ILevelsDataService
{
    private readonly IResourcesLoaderService _resourceLoader;
    private LevelsData _levelsData;
    private int _maxReachedLevel = 1;

    private const string LEVELS_DATA_KEY = "Assets/Data/Level/LevelsData.asset";
    private const string PREF_MAX_LEVEL = "MaxReachedLevel";

    public LevelsDataService(IResourcesLoaderService resourceLoader)
    {
        _resourceLoader = resourceLoader;
        LoadLevelsData().Forget();
        LoadMaxReachedLevel();
    }

    private async UniTaskVoid LoadLevelsData()
    {
        _levelsData = await _resourceLoader.LoadAsync<LevelsData>(LEVELS_DATA_KEY);

        if (_levelsData == null)
        {
            Debug.LogError("LevelsData not found! Please create LevelsData asset.");
        }
        else
        {
            Debug.Log($"LevelsData loaded: {_levelsData.GetTotalLevelCount()} levels");
        }
    }

    public LevelData GetLevelData(int levelNumber)
    {
        if (_levelsData == null)
        {
            Debug.LogError("LevelsData not loaded yet!");
            return null;
        }

        return _levelsData.GetLevel(levelNumber);
    }

    public int GetMaxReachedLevel() => _maxReachedLevel;

    public void SetMaxReachedLevel(int levelNumber)
    {
        if (levelNumber > _maxReachedLevel)
        {
            _maxReachedLevel = levelNumber;
            PlayerPrefs.SetInt(PREF_MAX_LEVEL, _maxReachedLevel);
            PlayerPrefs.Save();
            Debug.Log($"Max reached level updated: {_maxReachedLevel}");
        }
    }

    public int GetTotalLevelCount()
    {
        return _levelsData != null ? _levelsData.GetTotalLevelCount() : 0;
    }

    private void LoadMaxReachedLevel()
    {
        _maxReachedLevel = PlayerPrefs.GetInt(PREF_MAX_LEVEL, 1);
    }
}
