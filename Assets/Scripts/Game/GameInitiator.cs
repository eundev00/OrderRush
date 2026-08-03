using Cysharp.Threading.Tasks;
using VContainer.Unity;

public class GameInitiator : IStartable
{
    private readonly ILevelContextPresenter _levelPresenter;
    private readonly IDayProgressService _dayProgressService;
    private readonly IDayNightService _dayNightService;
    private readonly IAccountService _accountService;
    private readonly ISoundService _soundService;
    private readonly CardEffectApplier _cardEffectApplier;
    private readonly IDayEventService _dayEventService;
    private readonly IDayFlowService _dayFlowService;
    private readonly GameUIContextPresenter _uiContextPresenter;
    private readonly ISceneTransitionService _sceneTransition;

    public GameInitiator(
        ILevelContextPresenter levelPresenter,
        IDayProgressService dayProgressService,
        IDayNightService dayNightService,
        IAccountService accountService,
        ISoundService soundService,
        CardEffectApplier cardEffectApplier,
        IDayEventService dayEventService,
        IDayFlowService dayFlowService,
        GameUIContextPresenter uiContextPresenter,
        ISceneTransitionService sceneTransition)
    {
        _levelPresenter = levelPresenter;
        _dayProgressService = dayProgressService;
        _dayNightService = dayNightService;
        _accountService = accountService;
        _soundService = soundService;
        _cardEffectApplier = cardEffectApplier;
        _dayEventService = dayEventService;
        _dayFlowService = dayFlowService;
        _uiContextPresenter = uiContextPresenter;
        _sceneTransition = sceneTransition;
    }

    public async void Start()
    {
        _soundService.PlayBgm(AudioKeys.Bgm4);

        await _dayProgressService.Initialize();
        await _dayNightService.Initialize();

        int currentDay = _accountService.Account.CurrentDay;

        await _levelPresenter.LoadLevelContext(1);
        _uiContextPresenter.Initialize();
        await _cardEffectApplier.ApplyAllPurchasedCards();

        _dayEventService.Initialize(currentDay);

        _sceneTransition.NotifySceneReady();

        await _dayFlowService.RunFirstDayAsync(currentDay);
    }
}
