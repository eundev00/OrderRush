using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Services.UpdateService;

public class DiningTable : InteractableBase, IUpdatable
{
    [NotNull][SerializeField] Transform[] _plateSlots;
    [NotNull][SerializeField] DiningSeat[] _seats;

    private List<Plate> _currentPlates = new List<Plate>();

    private TableGaugeFactory _gaugeFactory;
    private TableGaugePresenter _tableGaugePresenter;
    private IUpdateSubscriptionService _updateService;

    private int _seatedCount = 0;
    private float _waitOrderTime = 60f;
    private float _elapsedWaitTime = 0f;

    private bool _isWaitingForOrder = false;
    private bool _isWaitingFood = false;


    [Inject]
    public void Construct(TableGaugeFactory gaugeFactory, IUpdateSubscriptionService updateService)
    {
        _gaugeFactory = gaugeFactory;
        _updateService = updateService;
    }

    void Awake()
    {
        for (int i = 0; i < _seats.Length; i++)
        {
            _seats[i].Init(this, i);
            _currentPlates.Add(null);
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
            StartWaitOrder();
        }

    }


    public void ManagedUpdate()
    {
        if (!_isWaitingForOrder) return;

        _elapsedWaitTime += Time.deltaTime;
        float progress = _elapsedWaitTime / _waitOrderTime;

        // 게이지 업데이트
        if (_tableGaugePresenter != null)
        {
            _tableGaugePresenter.SetProgress(progress);
        }

        // 시간 초과 시 처리
        if (_elapsedWaitTime >= _waitOrderTime)
        {
            OnWaitTimeout();
        }
    }

    public void StartWaitOrder()
    {
        _elapsedWaitTime = 0f;
        _isWaitingForOrder = true;
        _isWaitingFood = false;

        // 게이지 생성 및 표시
        if (_tableGaugePresenter == null)
        {
            _tableGaugePresenter = _gaugeFactory.Create(transform, new Vector3(0, 1.5f, 0));
        }
        _tableGaugePresenter.Show();

        // Update 구독
        _updateService.RegisterUpdatable(this);
    }

    public void StopWaitOrder()
    {
        _isWaitingForOrder = false;
        _isWaitingFood = false;
        _elapsedWaitTime = 0f;

        // 게이지 숨김
        if (_tableGaugePresenter != null)
        {
            _tableGaugePresenter.Hide();
        }

        // Update 구독 해제
        _updateService.UnregisterUpdatable(this);
    }

    private void ExtendGaugeTime(float seconds)
    {
        if (!_isWaitingFood)
        {
            Debug.LogWarning("[DiningTable] Cannot extend gauge during order waiting phase");
            return;
        }

        _elapsedWaitTime = Mathf.Max(0, _elapsedWaitTime - seconds);
        float newProgress = _elapsedWaitTime / _waitOrderTime;
        _tableGaugePresenter?.SetProgress(newProgress);

        Debug.Log($"[DiningTable] Gauge extended by {seconds}s. Remaining: {_waitOrderTime - _elapsedWaitTime}s");
    }

    private void OnWaitTimeout()
    {
        if (_isWaitingFood)
        {
            Debug.Log("[DiningTable] Food wait timeout! Customers leaving...");
        }
        else
        {
            Debug.Log("[DiningTable] Order wait timeout! Customers leaving...");
        }

        // 모든 앉은 손님 이탈 처리
        foreach (var seat in _seats)
        {
            if (seat.HasCustomer)
            {
                seat.CurrentCustomer.OnWaitTimeout();
                seat.Clear();
                _seatedCount--;
            }
        }

        StopWaitOrder();
    }

    public void PlacePlate(int seatIndex, Plate plate)
    {
        _currentPlates[seatIndex] = plate;
        plate.transform.SetParent(transform);
        plate.transform.position = _plateSlots[seatIndex].position;
    }

    public DiningSeat GetAvailableSeat()
    {
        return _seats.FirstOrDefault(seat => !seat.HasCustomer);
    }

    public bool IsEmptyTable()
    {
        return _seatedCount == 0;
    }


    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        Debug.Log($"[DiningTable] InteractAsync START - character: {character.name}, IsHolding: {character.IsHolding}, IsWaitingFood: {_isWaitingFood}");

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
        foreach (var seat in _seats)
        {
            if (seat.HasCustomer)
            {
                seat.CurrentCustomer.EnqueueTakeOrder();
            }
        }

        // 음식 대기로 전환 (게이지 리셋)
        _isWaitingFood = true;
        _elapsedWaitTime = 0f;
        if (_tableGaugePresenter != null)
        {
            _tableGaugePresenter.SetProgress(0f);
        }

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

        var ingredientDatas = plate.PlacedIngredients.Select(obj => obj.Data).ToList();

        foreach (var seat in _seats)
        {
            if (!seat.HasCustomer) continue;

            var customer = seat.CurrentCustomer;
            if (customer.Order != null &&
                !customer.Order.IsCompleted &&
                customer.Order.Recipe.IsComplete(ingredientDatas))
            {
                await character.PutDown();
                PlacePlate(seat.GetSeatIndex(), plate);

                customer.Order.Complete();
                ExtendGaugeTime(20f);
                customer.EnqueueEatAndLeave();

                CheckAllServed();

                Debug.Log($"[DiningTable] Food served to {customer.name}");
                return;
            }
        }

        Debug.Log("[DiningTable] No matching order found for this food");
    }

    private void CheckAllServed()
    {
        foreach (var seat in _seats)
        {
            if (seat.HasCustomer &&
                seat.CurrentCustomer.Order != null &&
                !seat.CurrentCustomer.Order.IsCompleted)
            {
                return;
            }
        }

        StopWaitOrder();
        Debug.Log("[DiningTable] All customers served");
    }

}
