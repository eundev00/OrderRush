using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;
using Services.UpdateService;
using UniRx;

public class DiningTable : InteractableBase, IUpdatable
{
    [NotNull][SerializeField] Transform[] _plateSlots;
    [NotNull][SerializeField] DiningSeat[] _seats;

    private List<Plate> _currentPlates = new List<Plate>();

    private WorldUIFactory _worldUIFactory;
    private ProgressGauge _tableProgressGauge;
    private IUpdateSubscriptionService _updateService;
    private IGameDataService _gameDataService;
    private IDayProgressService _dayProgressService;
    private ICustomerService _customerService;
    private ISoundService _soundService;
    private IPublisher<OrderNeededEvent> _orderNeededPublisher;
    private IPublisher<DirtyPlateEvent> _dirtyPlatePublisher;

    private int _seatedCount = 0;
    private float _elapsedWaitTime = 0f;

    private bool _isWaitingForOrder = false;
    private bool _isWaitingFood = false;

    private readonly CompositeDisposable _disposables = new();

    public int MaxSeats => _seats.Length;

    public bool HasCustomersNeedingOrder =>
        _seats.Any(s => s.HasCustomer && s.CurrentCustomer.OrderedRecipeID == -1);

    public bool HasDirtyPlate =>
        _seatedCount == 0 && _currentPlates.Any(p => p != null && p.IsDirty);

    public List<int> GetPendingRecipeIDs()
    {
        var ids = new List<int>();
        foreach (var seat in _seats)
        {
            if (!seat.HasCustomer) continue;
            var customer = seat.CurrentCustomer;
            if (customer.OrderedRecipeID == -1 || customer.IsServed) continue;
            ids.Add(customer.OrderedRecipeID);
        }
        return ids;
    }

    public float GetEarliestOrderTime(int recipeID)
    {
        float earliest = float.MaxValue;
        foreach (var seat in _seats)
        {
            if (!seat.HasCustomer) continue;
            var customer = seat.CurrentCustomer;
            if (customer.IsServed || customer.OrderedRecipeID != recipeID) continue;
            if (customer.OrderedTime < earliest)
                earliest = customer.OrderedTime;
        }
        return earliest;
    }

    [Inject]
    public void Construct(
        WorldUIFactory worldUIFactory,
        IUpdateSubscriptionService updateService,
        IGameDataService gameDataService,
        IDayProgressService dayProgressService,
        ICustomerService customerService,
        ISoundService soundService,
        IPublisher<OrderNeededEvent> orderNeededPublisher,
        IPublisher<DirtyPlateEvent> dirtyPlatePublisher,
        ISubscriber<DayEndedEvent> dayEndedSubscriber,
        ISubscriber<GameCleanupEvent> gameCleanupSubscriber)
    {
        _worldUIFactory = worldUIFactory;
        _updateService = updateService;
        _gameDataService = gameDataService;
        _dayProgressService = dayProgressService;
        _customerService = customerService;
        _soundService = soundService;
        _orderNeededPublisher = orderNeededPublisher;
        _dirtyPlatePublisher = dirtyPlatePublisher;

        dayEndedSubscriber
            .Subscribe(_ => OnDayEnded())
            .AddTo(_disposables);

        gameCleanupSubscriber
            .Subscribe(_ => OnGameCleanup())
            .AddTo(_disposables);
    }

    void Awake()
    {
        for (int i = 0; i < _seats.Length; i++)
        {
            _seats[i].Init(this, i);
            _currentPlates.Add(null);
        }
    }

    private void OnDayEnded()
    {
        StopWaitGauge();
    }

    private void OnGameCleanup()
    {
        StopWaitGauge();
        _seatedCount = 0;
        _isWaitingForOrder = false;
        _isWaitingFood = false;
        _elapsedWaitTime = 0f;

        for (int i = 0; i < _currentPlates.Count; i++)
        {
            _currentPlates[i] = null;
        }

        foreach (var seat in _seats)
        {
            seat.Clear();
        }
    }

    void OnDestroy()
    {
        _disposables?.Dispose();

        if (_tableProgressGauge != null)
        {
            _worldUIFactory.Release(PrefabKeys.TableGauge, _tableProgressGauge);
            _tableProgressGauge = null;
        }
    }

    public Transform GetSeatTransform(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= _seats.Length)
        {
            Debug.LogError($"[DiningTable] Invalid seat index: {seatIndex}");
            return null;
        }
        return _seats[seatIndex].SitPoint;
    }

    public void SeatCustomer(CustomerCharacter customer, int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= _seats.Length)
        {
            Debug.LogError($"[DiningTable] Invalid seat index: {seatIndex}");
            return;
        }
        _seats[seatIndex].SeatCustomer(customer);
        _seatedCount++;

        if (!_isWaitingForOrder)
        {
            _isWaitingForOrder = true;
            StartWaitGauge();
        }

        _orderNeededPublisher.Publish(new OrderNeededEvent(this));
    }


    public void ManagedUpdate()
    {
        if (!_isWaitingForOrder) return;

        _elapsedWaitTime += Time.deltaTime;
        float progress = _elapsedWaitTime / _gameDataService.Config.FoodWaitDuration;

        // 게이지 업데이트
        if (_tableProgressGauge != null)
        {
            _tableProgressGauge.SetProgress(progress);
        }

        // 시간 초과 시 처리
        if (_elapsedWaitTime >= _gameDataService.Config.FoodWaitDuration)
        {
            OnWaitTimeout();
        }
    }

    public void StartWaitGauge()
    {
        _elapsedWaitTime = 0f;
        _isWaitingForOrder = true;
        _isWaitingFood = false;

        _soundService?.PlaySfx(AudioKeys.bell1);

        // 게이지 생성 및 표시
        if (_tableProgressGauge == null)
        {
            _tableProgressGauge = _worldUIFactory.Create<ProgressGauge>(
                PrefabKeys.TableGauge,
                transform,
                new Vector3(0, 1.5f, 0));
        }
        _tableProgressGauge.Show();

        // Update 구독
        _updateService.RegisterUpdatable(this);
    }

    public void StopWaitGauge()
    {
        _isWaitingForOrder = false;
        _isWaitingFood = false;
        _elapsedWaitTime = 0f;

        // Update 구독 해제
        _updateService.UnregisterUpdatable(this);

        // 게이지 반납
        if (_tableProgressGauge != null)
        {
            _worldUIFactory.Release(PrefabKeys.TableGauge, _tableProgressGauge);
            _tableProgressGauge = null;
        }
    }

    private void ExtendGaugeTime()
    {
        if (!_isWaitingFood)
        {
            Debug.LogWarning("[DiningTable] Cannot extend gauge during order waiting phase");
            return;
        }

        float recoverAmount = _gameDataService.Config.FoodWaitDuration * _gameDataService.Config.PatienceRecoveryAmount;
        _elapsedWaitTime = Mathf.Max(0, _elapsedWaitTime - recoverAmount);
        float newProgress = _elapsedWaitTime / _gameDataService.Config.FoodWaitDuration;
        _tableProgressGauge?.SetProgress(newProgress);
    }

    private void OnWaitTimeout()
    {
        foreach (var seat in _seats)
        {
            if (seat.HasCustomer)
            {
                seat.CurrentCustomer.OnWaitTimeout();
            }
        }

        StopWaitGauge();
        _dayProgressService.FailDay();
    }

    public void PlacePlate(int seatIndex, Plate plate)
    {
        _currentPlates[seatIndex] = plate;
        plate.transform.SetParent(transform);
        plate.transform.position = _plateSlots[seatIndex].position;
    }

    public void CustomerLeaving(int seatIndex)
    {
        var seat = _seats[seatIndex];
        if (seat.HasCustomer)
        {
            seat.Clear();
            _seatedCount--;

            if (_currentPlates[seatIndex] != null)
            {
                _currentPlates[seatIndex].RemoveEatenFood();
            }

            if (IsEmptyTable())
            {
                _customerService.OnTableAvailable(this);
            }
            else if (HasDirtyPlate)
            {
                _dirtyPlatePublisher.Publish(new DirtyPlateEvent(this));
            }
        }
    }


    public bool IsEmptyTable()
    {
        if (_seatedCount > 0) return false;

        foreach (var plate in _currentPlates)
        {
            if (plate != null) return false;
        }

        return true;
    }


    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        Debug.Log($"[DiningTable] InteractAsync START - character: {character.name}, IsHolding: {character.IsHolding}, IsWaitingFood: {_isWaitingFood}");

        if (_seatedCount == 0)
        {
            if (!character.IsHolding)
            {
                // 더러운 접시 찾기
                for (int i = 0; i < _currentPlates.Count; i++)
                {
                    var plate = _currentPlates[i];
                    if (plate != null && plate.IsDirty)
                    {
                        await character.PickUp(plate);
                        _currentPlates[i] = null;

                        if (IsEmptyTable())
                        {
                            _customerService.OnTableAvailable(this);
                        }
                        else if (HasDirtyPlate)
                        {
                            _dirtyPlatePublisher.Publish(new DirtyPlateEvent(this));
                        }

                        return;
                    }
                }
            }

            // 빈 테이블에서는 주문/서빙으로 진행하지 않음
            return;
        }


        if (_isWaitingFood)
        {
            // 음식 대기 중 - 접시만 받음
            if (character.IsHolding &&
                character.CurrentCarriable.GetCarriableType() == CarriableType.Plate)
            {
                await ServeFood(character);
            }
            else
            {
                Debug.Log("[DiningTable] Waiting for food. Bring a plate!");
            }
        }
        else
        {
            // 주문 대기 중 - 빈 손으로만 주문 받기
            if (!character.IsHolding)
            {
                TakeOrders();
            }
            else
            {
                Debug.Log("[DiningTable] Take orders first!");
            }
        }

        Debug.Log("[DiningTable] InteractAsync END");
    }

    private void TakeOrders()
    {
        _isWaitingFood = true;
        _elapsedWaitTime = 0f;
        if (_tableProgressGauge != null)
        {
            _tableProgressGauge.SetProgress(0f);
        }

        foreach (var seat in _seats)
        {
            if (seat.HasCustomer)
            {
                seat.CurrentCustomer.EnqueueTakeOrder();
            }
        }

        // 음식 대기로 전환 (게이지 리셋)

        Debug.Log("[DiningTable] Switched to waiting for food. Gauge reset.");
    }

    private async UniTask ServeFood(CharacterBase character)
    {
        var plate = character.CurrentCarriable as Plate;
        if (plate == null)
        {
            Debug.LogWarning("[DiningTable] No plate to serve");
            return;
        }

        if (plate.MatchedRecipeID == -1) return;

        DiningSeat targetSeat = null;
        float earliestTime = float.MaxValue;
        foreach (var seat in _seats)
        {
            if (!seat.HasCustomer) continue;

            var customer = seat.CurrentCustomer;
            if (customer.IsServed || customer.OrderedRecipeID != plate.MatchedRecipeID) continue;

            if (customer.OrderedTime < earliestTime)
            {
                earliestTime = customer.OrderedTime;
                targetSeat = seat;
            }
        }

        if (targetSeat == null)
        {
            Debug.Log("[DiningTable] No matching order found for this food");
            return;
        }

        var targetCustomer = targetSeat.CurrentCustomer;
        targetCustomer.IsServed = true;
        ExtendGaugeTime();
        ProcessServingComplete();

        await character.PutDown();
        PlacePlate(targetSeat.GetSeatIndex(), plate);


        Debug.Log($"[DiningTable] Food served to {targetCustomer.name}");
    }

    private bool CheckAllServed()
    {
        foreach (var seat in _seats)
        {
            if (seat.HasCustomer &&
                seat.CurrentCustomer.OrderedRecipeID != -1 &&
                !seat.CurrentCustomer.IsServed)
                return false;
        }
        return true;
    }

    private void ProcessServingComplete()
    {
        if (!CheckAllServed()) return;

        foreach (var seat in _seats)
        {
            if (!seat.HasCustomer) continue;

            if (seat.CurrentCustomer != null)
            {
                seat.CurrentCustomer.EnqueueEatAndLeave();
            }
        }

        StopWaitGauge();
    }

}
