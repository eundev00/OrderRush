using Cysharp.Threading.Tasks;

public interface ILevelService
{
    LevelContext Context { get; }

    UniTask LoadLevelAsync(int levelNumber);
}
