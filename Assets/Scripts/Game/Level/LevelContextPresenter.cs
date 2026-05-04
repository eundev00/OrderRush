using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using UnityEngine;
using VContainer.Unity;

public class LevelContextPresenter : ILevelContextPresenter, ITickable, IDisposable
{
    private readonly IResourcesLoaderService _resourcesLoaderService;
    private readonly ILevelProgressService _levelsDataService;
    private readonly IOrderService _orderService;
    private readonly LevelFactory _levelFactory;
    private readonly ISubscriber<PaymentEvent> _paymentSubscriber;
    private readonly CompositeDisposable _disposable = new();

    public LevelData CurrentLevelData { get; private set; }
    public LevelContext CurrentLevelContext { get; private set; }
    public LevelProgressModel LevelProgressModel { get; private set; }

    private float _elapsedTime;

    public LevelContextPresenter(
        IResourcesLoaderService resourcesLoaderService,
        ILevelProgressService levelsDataService,
        IOrderService orderService,
        LevelFactory levelFactory,
        ISubscriber<PaymentEvent> paymentSubscriber)
    {
        _resourcesLoaderService = resourcesLoaderService;
        _levelsDataService = levelsDataService;
        _orderService = orderService;
        _levelFactory = levelFactory;
        _paymentSubscriber = paymentSubscriber;

        LevelProgressModel = new LevelProgressModel();

        // PaymentEvent 구독
        _paymentSubscriber
            .Subscribe(OnPaymentReceived)
            .AddTo(_disposable);
    }

    private void OnPaymentReceived(PaymentEvent evt)
    {
        LevelProgressModel.AddMoney(evt.Amount);
        Debug.Log($"[LevelContextPresenter] Payment received: +{evt.Amount} for {evt.RecipeName} (Total: {LevelProgressModel.CurrentMoney.Value}/{LevelProgressModel.TargetMoney})");

        CheckLevelCompleted();
    }

    public async UniTask LoadLevelContext(int levelNumber)
    {
        // 1. 레벨 데이터 가져오기
        CurrentLevelData = _levelsDataService.GetLevelData(levelNumber);
        if (CurrentLevelData == null)
        {
            Debug.LogError($"Level {levelNumber} data not found!");
            return;
        }

        // 2. 레벨 맵 로드
        var levelContext = await _levelFactory.CreateLevelContext(levelNumber);
        if (levelContext == null)
        {
            Debug.LogError($"Failed to load level map: Level{levelNumber}");
            return;
        }
        CurrentLevelContext = levelContext;
        // 3. Model 초기화
        LevelProgressModel.Initialize(CurrentLevelData.TargetMoney, CurrentLevelData.TimeLimit);
        _elapsedTime = 0f;

        Debug.Log($"Level {CurrentLevelData.LevelNumber} loaded: {CurrentLevelData.LevelName}");
    }

    public void Tick()
    {
        if (!LevelProgressModel.IsLevelActive.Value || CurrentLevelData == null) return;

        _elapsedTime += Time.deltaTime;

        // 남은 시간 업데이트
        if (CurrentLevelData.TimeLimit > 0)
        {
            float remainingTime = Mathf.Max(0, CurrentLevelData.TimeLimit - _elapsedTime);
            LevelProgressModel.UpdateRemainingTime(remainingTime);

            // 시간 제한 체크
            if (LevelProgressModel.IsTimeExpired())
            {
                CheckLevelFailed();
            }
        }
    }

    private void CheckLevelCompleted()
    {
        if (LevelProgressModel.IsCompleted())
        {
            LevelProgressModel.SetLevelInactive();
            _levelsDataService.SetMaxReachedLevel(CurrentLevelData.LevelNumber + 1);

            Debug.Log($"[LevelContextPresenter] ===== LEVEL {CurrentLevelData.LevelNumber} COMPLETED! =====");
            Debug.Log($"[LevelContextPresenter] Final Money: {LevelProgressModel.CurrentMoney.Value}/{LevelProgressModel.TargetMoney}");
        }
    }

    private void CheckLevelFailed()
    {
        if (!LevelProgressModel.IsCompleted())
        {
            LevelProgressModel.SetLevelInactive();

            Debug.Log($"[LevelContextPresenter] ===== LEVEL {CurrentLevelData.LevelNumber} FAILED! =====");
            Debug.Log($"[LevelContextPresenter] Final Money: {LevelProgressModel.CurrentMoney.Value}/{LevelProgressModel.TargetMoney}");
            Debug.Log($"[LevelContextPresenter] Time expired!");
        }
    }

    public void Dispose()
    {
        _disposable?.Dispose();
        LevelProgressModel?.Dispose();

        if (CurrentLevelContext != null && CurrentLevelData != null)
        {
            _levelFactory.ReleaseLevelContext(CurrentLevelData.LevelNumber);
        }
    }
}
