using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using UnityEngine;
using VContainer.Unity;

public class HudPresenter : IStartable, IDisposable
{
    readonly IDayProgressService _dayProgressService;
    readonly IAccountService _accountService;
    readonly ISoundService _soundService;
    readonly ISceneTransitionService _sceneTransition;
    readonly IPublisher<SpawnTestItemEvent> _spawnTestItemPublisher;
    readonly HudView _hudView;
    readonly CompositeDisposable _disposable = new();

    public HudPresenter(
        IDayProgressService dayProgressService,
        IAccountService accountService,
        ISoundService soundService,
        ISceneTransitionService sceneTransition,
        IPublisher<SpawnTestItemEvent> spawnTestItemPublisher,
        HudView hudView)
    {
        _dayProgressService = dayProgressService;
        _accountService = accountService;
        _soundService = soundService;
        _sceneTransition = sceneTransition;
        _spawnTestItemPublisher = spawnTestItemPublisher;
        _hudView = hudView;
    }

    public void Start()
    {
        var dayContext = _dayProgressService.CurrentDayContext;

        _hudView.SetTimeGauge(1f);

        dayContext.TimeBarElapsed
            .Subscribe(elapsed =>
            {
                if (dayContext.TimeBarDuration > 0)
                {
                    float remainingRatio = Mathf.Clamp01(1f - (elapsed / dayContext.TimeBarDuration));
                    _hudView.SetTimeGauge(remainingRatio);
                }
            })
            .AddTo(_disposable);

        Observable.CombineLatest(
                _accountService.Account.Coins,
                dayContext.EarnedCoins,
                (account, earned) => account + earned)
            .Subscribe(total => _hudView.SetCoin(total))
            .AddTo(_disposable);

        dayContext.DayNumber
            .Subscribe(dayNumber =>
            {
                if (dayNumber <= 0)
                    return;

                var daysData = _dayProgressService.CurrentDaysData;
                int maxCustomers = daysData.GetMaxCustomers(dayNumber);

                _hudView.SetDay(dayNumber);
                _hudView.SetMaxCustomers(maxCustomers);
            })
            .AddTo(_disposable);

        _hudView.SetHomeButtonListener(OnHomeButtonClicked);
        _hudView.SetTestButtonListener(OnTestButtonClicked);
    }

    private void OnTestButtonClicked()
    {
        _spawnTestItemPublisher.Publish(new SpawnTestItemEvent());
    }

    private void OnHomeButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        _sceneTransition.TransitionAsync("GameplayScene", "LobbyScene").Forget();
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}
