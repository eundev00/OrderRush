using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using UnityEngine;
using VContainer.Unity;

public class HudPresenter : IStartable, IDisposable
{
    readonly IDayProgressService _dayProgressService;
    readonly IAccountService _accountService;
    readonly ISubscriber<GameCleanupEvent> _gameCleanupSubscriber;
    readonly HudView _hudView;
    readonly WorldUIFactory _worldUIFactory;
    readonly CompositeDisposable _disposable = new();

    public HudPresenter(
        IDayProgressService dayProgressService,
        IAccountService accountService,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber,
        HudView hudView,
        WorldUIFactory worldUIFactory)
    {
        _dayProgressService = dayProgressService;
        _accountService = accountService;
        _gameCleanupSubscriber = gameCleanupSubscriber;
        _hudView = hudView;
        _worldUIFactory = worldUIFactory;
    }

    public void Start()
    {
        var dayContext = _dayProgressService.CurrentDayContext;

        UpdateDayInfo();

        dayContext.TimeBarElapsed
            .Subscribe(elapsed =>
            {
                float remainingRatio = 1f - (elapsed / dayContext.TimeBarDuration);
                _hudView.SetTimeGauge(remainingRatio);
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

    private async void OnHomeButtonClicked()
    {
        var player = GameObject.FindObjectOfType<PlayerCharacter>();
        if (player == null)
        {
            Debug.LogWarning("[HudPresenter] PlayerCharacter not found for test");
            return;
        }

        Debug.Log("[HudPresenter] TEST: Creating FloatingCoinFX above player (with tip - 2 coins)");

        // 첫 번째 코인 시작
        var coinObj1 = _worldUIFactory.Create(PrefabKeys.FloatingCoinFX);
        var coinFX1 = coinObj1.GetComponent<FloatingCoinFX>();
        var task1 = coinFX1.PlayAnimation(CancellationToken.None);

        // 시간차를 두고 두 번째 코인 시작
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.2f));

        var coinObj2 = _worldUIFactory.Create(PrefabKeys.FloatingCoinFX);
        var coinFX2 = coinObj2.GetComponent<FloatingCoinFX>();
        var task2 = coinFX2.PlayAnimation(CancellationToken.None);

        // 둘 다 끝날 때까지 대기
        await UniTask.WhenAll(task1, task2);

        _worldUIFactory.Release(PrefabKeys.FloatingCoinFX, coinObj1);
        _worldUIFactory.Release(PrefabKeys.FloatingCoinFX, coinObj2);

        Debug.Log("[HudPresenter] TEST: FloatingCoinFX test complete (2 coins played)");
    }

    private void UpdateDayInfo()
    {
        var dayContext = _dayProgressService.CurrentDayContext;
        var daysData = _dayProgressService.CurrentDaysData;

        int dayNumber = dayContext.DayNumber;
        int maxCustomers = daysData.GetMaxCustomers(dayNumber);

        _hudView.SetDay(dayNumber);
        _hudView.SetMaxCustomers(maxCustomers);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

}
