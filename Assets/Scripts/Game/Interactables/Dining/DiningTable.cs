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

        // 게이지 생성 및 표시
        if (_tableGaugePresenter == null)
        {
            _tableGaugePresenter = (TableGaugePresenter)_gaugeFactory.Create(transform, new Vector3(0, 1.5f, 0));
        }
        _tableGaugePresenter.Show();
        _tableGaugePresenter.UpdatePosition();

        // Update 구독
        _updateService.RegisterUpdatable(this);
    }

    public void StopWaitOrder()
    {
        _isWaitingForOrder = false;
        _elapsedWaitTime = 0f;

        // 게이지 숨김
        if (_tableGaugePresenter != null)
        {
            _tableGaugePresenter.Hide();
        }

        // Update 구독 해제
        _updateService.UnregisterUpdatable(this);
    }

    private void OnWaitTimeout()
    {
        Debug.Log("[DiningTable] Wait timeout! Customers leaving...");

        // 모든 앉은 손님 이탈 처리
        foreach (var seat in _seats)
        {
            if (seat.HasCustomer)
            {
                seat.CurrentCustomer.Leave();
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
        Debug.Log($"[DiningTable] InteractAsync START - character: {character.name}, IsHolding: {character.IsHolding}");

        // TODO: 조건 로직 직접 수정 필요
        if (character.IsHolding)
        {
            var carriableType = character.CurrentCarriable.GetCarriableType();
            if (carriableType == CarriableType.Plate)
            {
                await ServeFood(character);
            }
        }

        // 주문 받기
        TakeOrders();

        Debug.Log("[DiningTable] InteractAsync END");
    }

    private void TakeOrders()
    {
        StopWaitOrder();
        foreach (var seat in _seats)
        {
            if (seat.HasCustomer)
            {
                seat.CurrentCustomer.TryTakeOrder();
            }
        }
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

        // 모든 좌석을 순회하며 주문과 맞는 손님 찾기
        // foreach (var seat in _seats)
        // {
        //     if (!seat.HasCustomer) continue;

        //     var customer = seat.CurrentCustomer;
        //     if (customer.Order != null && customer.Order.Recipe.IsComplete(ingredientDatas))
        //     {
        //         // 주문과 일치! 서빙 처리
        //         await character.PutDown();
        //         PlacePlate(seat.GetSeatIndex(), plate);

        //         // Order 완료 처리
        //         customer.Order.Complete();

        //         // 손님이 음식 먹고 나가기 (접시를 넘겨줌)
        //         customer.EatAndLeave(plate);

        //         Debug.Log($"[DiningTable] Food served to {customer.name}");
        //         return;
        //     }
        // }

        Debug.Log("[DiningTable] No matching order found for this food");
    }

    private async UniTask PickUpPlate(CharacterBase character)
    {
        Plate plate = null;
        int plateIndex = -1;

        for (int i = 0; i < _currentPlates.Count; i++)
        {
            if (_currentPlates[i] != null)
            {
                plate = _currentPlates[i];
                plateIndex = i;
                break;
            }
        }

        if (plate == null) return;

        await character.PickUp(plate);
        _currentPlates[plateIndex] = null;

        await UniTask.CompletedTask;
    }
}
