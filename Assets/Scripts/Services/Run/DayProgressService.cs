using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Data;
using OrderRush.Models;
using Services.UpdateService;
using UnityEngine;

public class DayProgressService : IDayProgressService, IUpdatable
{
    private readonly IGameDataService _gameDataService;
    private readonly IUpdateSubscriptionService _updateService;
    private readonly ISubscriber<PaymentEvent> _paymentSubscriber;
    private readonly IPublisher<DayEndedEvent> _dayEndedPublisher;
    private readonly IPublisher<GameCleanupEvent> _gameCleanupPublisher;
    private int _currentRun;
    private DayContext _currentDayContext;
    private DaysData _currentDaysData;
    private bool _isDayActive;
    private IDisposable _paymentSubscription;

    public int CurrentRun => _currentRun;
    public DayContext CurrentDayContext => _currentDayContext;
    public DaysData CurrentDaysData => _currentDaysData;

    public DayProgressService(
        IGameDataService gameDataService,
        IUpdateSubscriptionService updateService,
        ISubscriber<PaymentEvent> paymentSubscriber,
        IPublisher<DayEndedEvent> dayEndedPublisher,
        IPublisher<GameCleanupEvent> gameCleanupPublisher)
    {
        _gameDataService = gameDataService;
        _updateService = updateService;
        _paymentSubscriber = paymentSubscriber;
        _dayEndedPublisher = dayEndedPublisher;
        _gameCleanupPublisher = gameCleanupPublisher;
        _currentDayContext = new DayContext();
    }

    public async UniTask Initialize()
    {
        _paymentSubscription?.Dispose();

        _currentRun = 1;
        _currentDaysData = _gameDataService.Days;
        _updateService.RegisterUpdatable(this);
        _paymentSubscription = _paymentSubscriber.Subscribe(OnPayment);

        _currentDayContext.Reset();
        _isDayActive = false;

        await UniTask.CompletedTask;
    }

    private void OnPayment(PaymentEvent evt)
    {
        if (_currentDayContext != null)
        {
            _currentDayContext.EarnedCoins.Value += evt.Amount;
        }
    }

    public void InitDay(int dayNumber)
    {
        if (_currentDaysData == null)
        {
            Debug.LogError("[DayProgressService] InitDay failed: _currentDaysData is null. Re-initializing...");
            _currentDaysData = _gameDataService.Days;

            if (_currentDaysData == null)
            {
                Debug.LogError("[DayProgressService] GameDataService.Days is also null!");
                return;
            }
        }

        _currentDayContext.DayNumber.Value = dayNumber;
        _currentDayContext.TimeBarDuration = _currentDaysData.GetTimeBarDuration(dayNumber);
    }

    public void StartDayTimer()
    {
        _isDayActive = true;
    }

    public void ManagedUpdate()
    {
        if (!_isDayActive || _currentDayContext == null)
            return;

        float elapsed = _currentDayContext.TimeBarElapsed.Value + Time.deltaTime;

        if (elapsed >= _currentDayContext.TimeBarDuration)
        {
            elapsed = _currentDayContext.TimeBarDuration;
        }

        _currentDayContext.TimeBarElapsed.Value = elapsed;
    }

    public void CompleteDay()
    {
        if (!_isDayActive) return;
        _isDayActive = false;
        _currentDayContext.IsCompleted = true;

        _dayEndedPublisher.Publish(new DayEndedEvent(
            _currentDayContext.DayNumber.Value + 1,
            true,
            _currentDayContext.EarnedCoins.Value));

        _currentDayContext.EarnedCoins.Value = 0;
    }

    public void FailDay()
    {
        if (!_isDayActive) return;
        _isDayActive = false;
        _currentDayContext.IsCompleted = false;
        _dayEndedPublisher.Publish(new DayEndedEvent(
            _currentDayContext.DayNumber.Value,
            false,
            _currentDayContext.EarnedCoins.Value));
    }

    public void RestartDay()
    {
        if (_currentDayContext == null)
            return;

        _gameCleanupPublisher.Publish(new GameCleanupEvent());

        int dayNumber = _currentDayContext.DayNumber.Value;
        _currentDayContext.Reset();
        _currentDayContext.DayNumber.Value = dayNumber;
        _currentDayContext.TimeBarDuration = _currentDaysData.GetTimeBarDuration(dayNumber);
        _isDayActive = true;
    }

    public void SetNextDay()
    {
        if (_currentDayContext == null)
            return;

        int nextDayNumber = _currentDayContext.DayNumber.Value + 1;
        _currentDayContext.Reset();
        InitDay(nextDayNumber);

        _gameCleanupPublisher.Publish(new GameCleanupEvent());
    }

    public void CompleteRun()
    {
        _isDayActive = false;
    }

    public void Dispose()
    {
        _updateService.UnregisterUpdatable(this);
        _paymentSubscription?.Dispose();
    }
}
