using Cysharp.Threading.Tasks;

public interface ILevelContextPresenter
{
    UniTask LoadLevelContext(int levelNumber);
}
