using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameInitiator : IStartable
{
    private readonly LevelContextPresenter _levelPresenter;

    [Inject]
    public GameInitiator(LevelContextPresenter levelPresenter)
    {
        _levelPresenter = levelPresenter;
    }

    public async void Start()
    {
        Debug.Log("GameInitiator: Initializing game...");

        // 레벨 1 로드
        await _levelPresenter.LoadLevelContext(1);

        Debug.Log("GameInitiator: Game initialized!");
    }
}
