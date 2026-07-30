using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class HudPresenter : IStartable, IDisposable
{
    readonly IDayProgressService _dayProgressService;
    readonly IAccountService _accountService;
    readonly ISoundService _soundService;
    readonly HudView _hudView;
    readonly WorldUIFactory _worldUIFactory;
    readonly CompositeDisposable _disposable = new();

    public HudPresenter(
        IDayProgressService dayProgressService,
        IAccountService accountService,
        ISoundService soundService,
        HudView hudView,
        WorldUIFactory worldUIFactory)
    {
        _dayProgressService = dayProgressService;
        _accountService = accountService;
        _soundService = soundService;
        _hudView = hudView;
        _worldUIFactory = worldUIFactory;
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
    }

    private void OnHomeButtonClicked()
    {
        // TODO: 테스트 코드 — 확인 후 제거
        var player = UnityEngine.Object.FindAnyObjectByType<PlayerCharacter>();
        if (player != null)
        {
            TestCoinFX(player).Forget();
        }

        // _soundService.PlaySfx(AudioKeys.commonbutton);
        // SceneManager.UnloadSceneAsync("GameplayScene");
        // SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    }

    private async UniTaskVoid TestCoinFX(PlayerCharacter player)
    {
        var headPos = player.transform.position + Vector3.up * 3f;
        var coinObj = _worldUIFactory.Create(PrefabKeys.FloatingCoinFX);
        coinObj.transform.position = headPos;
        var coinFX = coinObj.GetComponent<FloatingCoinFX>();
        await coinFX.PlayAnimation(CancellationToken.None);
        _worldUIFactory.Release(PrefabKeys.FloatingCoinFX, coinObj);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

}
