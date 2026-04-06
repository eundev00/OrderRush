public interface ILevelsDataService
{
    LevelData GetLevelData(int levelNumber);
    int GetMaxReachedLevel();
    void SetMaxReachedLevel(int levelNumber);
    int GetTotalLevelCount();
}
