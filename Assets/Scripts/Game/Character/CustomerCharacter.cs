using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;

public class CustomerCharacter : CharacterBase
{
    public Order Order { get; private set; }
    public DiningTable AssignedTable { get; private set; }
    public int AssignedSeatIndex { get; private set; }

    public const float DEFAULT_TIME_LIMIT = 60f;
    private const float EAT_DURATION = 5f;
    public const float WAIT_TIME_LIMIT = 60f;

    private IOrderService _orderService;
    private Vector3 _spawnPosition;

    private IPublisher<PaymentEvent> _paymentPublisher;

    [Inject]
    public void Construct(IPublisher<PaymentEvent> paymentPublisher, IOrderService orderService)
    {
        _paymentPublisher = paymentPublisher;
        _orderService = orderService;
    }

    public void SetSpawnPosition(Vector3 position)
    {
        _spawnPosition = position;
    }

    public void GoToSeat(DiningTable targetTable, int seatIndex)
    {
        AssignedTable = targetTable;
        AssignedSeatIndex = seatIndex;

        // 이동 → 착석 → 주문 생성
        var targetSeat = targetTable.GetSeatTransform(seatIndex);
        if (targetSeat == null)
        {
            Debug.LogError($"[CustomerCharacter] Invalid seat index: {seatIndex}");
            return;
        }
        EnqueueAction(new MoveAction(_mover, targetSeat.position, _animator));
        EnqueueAction(new SitAction(this, AssignedTable, AssignedSeatIndex));
        EnqueueAction(new WaitForOrderAction());
    }

    public async void EatAndLeave(Plate plate)
    {
        // Debug.Log($"[CustomerCharacter] Starting to eat...");

        // // 5초 동안 음식 먹기
        // await UniTask.Delay((int)(EAT_DURATION * 1000));

        // Debug.Log($"[CustomerCharacter] Finished eating, leaving...");

        // // 접시 비우기 (의자에서 일어나기 전)
        // if (plate != null)
        // {
        //     plate.ClearIngredients();
        //     Debug.Log($"[CustomerCharacter] Plate cleared");
        // }

        // if (Order != null && _paymentPublisher != null)
        // {
        //     var paymentEvent = new PaymentEvent(Order.Recipe.Price, Order.Recipe.RecipeName);
        //     _paymentPublisher.Publish(paymentEvent);
        //     Debug.Log($"[CustomerCharacter] Payment event published: ${Order.Recipe.Price} for {Order.Recipe.RecipeName}");
        // }

        // // 의자에서 일어나기 (NavMeshAgent 다시 활성화)
        // EnableNavMeshAgent();
        // AssignedTable.Clear();

        // // 스폰 포지션으로 이동
        // EnqueueAction(new MoveAction(_mover, _spawnPosition, _animator));

        // // 이동 완료 후 사라지기 (ActionExecutor의 큐가 비워지면)
        // await UniTask.WaitUntil(() => !IsExecuting);

        // Debug.Log($"[CustomerCharacter] Destroying...");
        // Destroy(gameObject);
    }

    public void Leave()
    {
        Debug.Log($"[CustomerCharacter] Leaving table...");

        // 기존 액션 모두 취소
        ClearActions();

        // LeaveAction 추가 (이동 후 Destroy)
        EnqueueAction(new LeaveAction(this, _spawnPosition, _mover, _animator));
    }

    public void OnWaitTimeout()
    {
        Debug.Log($"[CustomerCharacter] Wait timeout! Leaving angry...");
        Leave();
    }


    public void SetWaitGauge(float ratio)
    {
    }

    public bool TryTakeOrder()
    {
        if (_actionExecutor.CurrentAction is WaitForOrderAction)
        {
            _actionExecutor.CancelCurrentAction();
            Order = _orderService.AddOrder();
            EnqueueAction(new WaitForFoodAction());
            Debug.Log($"[CustomerCharacter] Order taken: {Order.Recipe.RecipeName}");
            return true;
        }
        return false;
    }

}
