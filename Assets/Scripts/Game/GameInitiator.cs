using System.Threading;
using Cysharp.Threading.Tasks;
using OrderRush.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameInitiator : IStartable
{
    private readonly IGameDataService _gameDataService;
    private readonly ILevelContextPresenter _levelPresenter;
    private readonly IDayProgressService _dayProgressService;
    private readonly ICustomerService _customerService;

    public GameInitiator(
        IGameDataService gameDataService,
        ILevelContextPresenter levelPresenter,
        IDayProgressService dayProgressService,
        ICustomerService customerService)
    {
        _gameDataService = gameDataService;
        _levelPresenter = levelPresenter;
        _dayProgressService = dayProgressService;
        _customerService = customerService;
    }

    public async void Start()
    {
        Debug.Log("GameInitiator: Initializing game...");

        await _dayProgressService.Initialize();
        _dayProgressService.StartDay(1);

        await _levelPresenter.LoadLevelContext(1);

        _customerService.Initialize();

        Debug.Log("GameInitiator: Game initialized!");
    }
}
