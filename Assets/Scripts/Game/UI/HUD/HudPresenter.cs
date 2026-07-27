using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class HudPresenter : IStartable, IDisposable
{
    readonly IDayProgressService _dayProgressService;
    readonly IAccountService _accountService;
    readonly ISoundService _soundService;
    readonly ISubscriber<GameCleanupEvent> _gameCleanupSubscriber;
    readonly HudView _hudView;
    readonly CompositeDisposable _disposable = new();

    public HudPresenter(
        IDayProgressService dayProgressService,
        IAccountService accountService,
        ISoundService soundService,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber,
        HudView hudView)
    {
        _dayProgressService = dayProgressService;
        _accountService = accountService;
        _soundService = soundService;
        _gameCleanupSubscriber = gameCleanupSubscriber;
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

        _accountService.Account.Coins
            .Subscribe(coins => _hudView.SetCoin(coins))
            .AddTo(_disposable);

        _gameCleanupSubscriber
            .Subscribe(_ => UpdateDayInfo())
            .AddTo(_disposable);

        _hudView.SetHomeButtonListener(OnHomeButtonClicked);
    }

    private void OnHomeButtonClicked()
    {
        _soundService.PlaySfx(AudioKeys.commonbutton);
        SceneManager.UnloadSceneAsync("GameplayScene");
        SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    }

    private void UpdateDayInfo()
    {
        var dayContext = _dayProgressService.CurrentDayContext;
        var daysData = _dayProgressService.CurrentDaysData;

        int dayNumber = dayContext.DayNumber;

        if (dayNumber <= 0)
            return;

        int maxCustomers = daysData.GetMaxCustomers(dayNumber);

        _hudView.SetDay(dayNumber);
        _hudView.SetMaxCustomers(maxCustomers);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

}
