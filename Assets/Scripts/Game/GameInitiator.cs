using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameInitiator : IStartable
{
    private readonly ILevelContextPresenter _levelPresenter;
    private readonly IDayProgressService _dayProgressService;
    private readonly IDayNightService _dayNightService;
    private readonly ICustomerService _customerService;
    private readonly IAccountService _accountService;
    private readonly ISoundService _soundService;
    private readonly CardEffectApplier _cardEffectApplier;
    private readonly StaffManager _staffManager;

    public GameInitiator(
        ILevelContextPresenter levelPresenter,
        IDayProgressService dayProgressService,
        IDayNightService dayNightService,
        ICustomerService customerService,
        IAccountService accountService,
        ISoundService soundService,
        CardEffectApplier cardEffectApplier,
        StaffManager staffManager)
    {
        _levelPresenter = levelPresenter;
        _dayProgressService = dayProgressService;
        _dayNightService = dayNightService;
        _customerService = customerService;
        _accountService = accountService;
        _soundService = soundService;
        _cardEffectApplier = cardEffectApplier;
        _staffManager = staffManager;
    }

    public async void Start()
    {
        Debug.Log($"[GameInitiator] Start() called - DayProgressService instance: {_dayProgressService.GetHashCode()}");

        _soundService.PlayBgm(AudioKeys.Bgm4);

        await _dayProgressService.Initialize();
        await _dayNightService.Initialize();

        int currentDay = _accountService.Account.CurrentDay;
        _dayProgressService.StartDay(currentDay);

        await _levelPresenter.LoadLevelContext(1);

        await _cardEffectApplier.ApplyAllPurchasedCards();

        _customerService.Initialize();
        _staffManager.Initialize();

        Debug.Log("GameInitiator: Game initialized!");
    }
}
