using Cysharp.Threading.Tasks;
using VContainer.Unity;

public class GameInitiator : IStartable
{
    private readonly ILevelService _levelService;
    private readonly IDayProgressService _dayProgressService;
    private readonly IDayNightService _dayNightService;
    private readonly IAccountService _accountService;
    private readonly ISoundService _soundService;
    private readonly CardEffectApplier _cardEffectApplier;
    private readonly IDayEventService _dayEventService;
    private readonly IDayFlowService _dayFlowService;
    private readonly GameUIContext _gameUIContext;
    private readonly IPopupService _popupService;
    private readonly ISceneTransitionService _sceneTransition;
    private readonly StreetTrafficService _streetTrafficService;

    public GameInitiator(
        ILevelService levelService,
        IDayProgressService dayProgressService,
        IDayNightService dayNightService,
        IAccountService accountService,
        ISoundService soundService,
        CardEffectApplier cardEffectApplier,
        IDayEventService dayEventService,
        IDayFlowService dayFlowService,
        GameUIContext gameUIContext,
        IPopupService popupService,
        ISceneTransitionService sceneTransition,
        StreetTrafficService streetTrafficService)
    {
        _levelService = levelService;
        _dayProgressService = dayProgressService;
        _dayNightService = dayNightService;
        _accountService = accountService;
        _soundService = soundService;
        _cardEffectApplier = cardEffectApplier;
        _dayEventService = dayEventService;
        _dayFlowService = dayFlowService;
        _gameUIContext = gameUIContext;
        _popupService = popupService;
        _sceneTransition = sceneTransition;
        _streetTrafficService = streetTrafficService;
    }

    public async void Start()
    {
        _soundService.PlayBgm(AudioKeys.Bgm4);

        await _dayProgressService.Initialize();
        await _dayNightService.Initialize();

        int currentDay = _accountService.Account.CurrentDay;

        await _levelService.LoadLevelAsync(1);
        _popupService.SetCanvasRoot(_gameUIContext.PopupRoot);
        await _cardEffectApplier.ApplyAllPurchasedCards();

        _streetTrafficService.Initialize();
        _dayEventService.Initialize(currentDay);

        _sceneTransition.NotifySceneReady();

        await _dayFlowService.RunFirstDayAsync(currentDay);
    }
}
