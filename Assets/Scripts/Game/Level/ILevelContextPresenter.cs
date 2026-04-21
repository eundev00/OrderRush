using Cysharp.Threading.Tasks;

public interface ILevelContextPresenter
{
    LevelData CurrentLevelData { get; }
    LevelContext CurrentLevelContext { get; }
    LevelProgressModel LevelProgressModel { get; }

    UniTask LoadLevelContext(int levelNumber);
}
