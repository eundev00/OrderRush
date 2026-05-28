using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Data;
using OrderRush.Models;
using Services.UpdateService;
using UnityEngine;

namespace OrderRush.Services
{
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
            _currentRun = 1;
            _currentDaysData = _gameDataService.Days;
            _updateService.RegisterUpdatable(this);
            _paymentSubscription = _paymentSubscriber.Subscribe(OnPayment);
            await UniTask.CompletedTask;
        }

        private void OnPayment(PaymentEvent evt)
        {
            if (_currentDayContext != null)
            {
                _currentDayContext.EarnedCoins.Value += evt.Amount;
            }
        }

        public void StartDay(int dayNumber)
        {
            _currentDayContext.DayNumber = dayNumber;
            _currentDayContext.TimeBarDuration = _currentDaysData.GetTimeBarDuration(dayNumber);
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
            _isDayActive = false;
            _currentDayContext.IsCompleted = true;

            _dayEndedPublisher.Publish(new DayEndedEvent(_currentDayContext.DayNumber + 1));
        }

        public void FailDay()
        {
            _isDayActive = false;
            _currentDayContext.IsCompleted = false;
            _dayEndedPublisher.Publish(new DayEndedEvent(_currentDayContext.DayNumber));
        }

        public void RestartDay()
        {
            if (_currentDayContext == null)
                return;

            _gameCleanupPublisher.Publish(new GameCleanupEvent());

            _currentDayContext.TimeBarElapsed.Value = 0f;
            _currentDayContext.EarnedCoins.Value = 0;
            _currentDayContext.SpawnedCustomers.Value = 0;
            _currentDayContext.IsCompleted = false;
            _isDayActive = true;
        }

        public void NextDay()
        {
            if (_currentDayContext == null)
                return;

            int nextDayNumber = _currentDayContext.DayNumber + 1;
            StartDay(nextDayNumber);

            _currentDayContext.TimeBarElapsed.Value = 0f;
            _currentDayContext.EarnedCoins.Value = 0;
            _currentDayContext.SpawnedCustomers.Value = 0;
            _currentDayContext.IsCompleted = false;

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
}
