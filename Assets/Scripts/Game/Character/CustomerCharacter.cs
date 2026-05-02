using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;

public class CustomerCharacter : CharacterBase, IInteractable
{
    public Order Order { get; private set; }
    public DiningSeat AssignedSeat { get; private set; }

    public const float DEFAULT_TIME_LIMIT = 60f;
    private const float EAT_DURATION = 5f;
    public const float WAIT_TIME_LIMIT = 60f;


    private IOrderService _orderService;
    private Vector3 _spawnPosition;
    private IPublisher<PaymentEvent> _paymentPublisher;
    public Transform InteractPoint => AssignedSeat != null ? AssignedSeat.Table.InteractPoint : transform;

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

    public void GoToSeat(DiningSeat targetSeat)
    {
        AssignedSeat = targetSeat;

        // 이동 → 착석 → 주문 생성
        EnqueueAction(new MoveAction(_mover, targetSeat.SitPoint.position, _animator));
        EnqueueAction(new SitAction(this, targetSeat));
        EnqueueAction(new WaitForOrderAction(this));
    }

    public async void EatAndLeave(Plate plate)
    {
        Debug.Log($"[CustomerCharacter] Starting to eat...");

        // 5초 동안 음식 먹기
        await UniTask.Delay((int)(EAT_DURATION * 1000));

        Debug.Log($"[CustomerCharacter] Finished eating, leaving...");

        // 접시 비우기 (의자에서 일어나기 전)
        if (plate != null)
        {
            plate.ClearIngredients();
            Debug.Log($"[CustomerCharacter] Plate cleared");
        }

        if (Order != null && _paymentPublisher != null)
        {
            var paymentEvent = new PaymentEvent(Order.Recipe.Price, Order.Recipe.RecipeName);
            _paymentPublisher.Publish(paymentEvent);
            Debug.Log($"[CustomerCharacter] Payment event published: ${Order.Recipe.Price} for {Order.Recipe.RecipeName}");
        }

        // 의자에서 일어나기 (NavMeshAgent 다시 활성화)
        EnableNavMeshAgent();
        AssignedSeat.Clear();

        // 스폰 포지션으로 이동
        EnqueueAction(new MoveAction(_mover, _spawnPosition, _animator));

        // 이동 완료 후 사라지기 (ActionExecutor의 큐가 비워지면)
        await UniTask.WaitUntil(() => !IsExecuting);

        Debug.Log($"[CustomerCharacter] Destroying...");
        Destroy(gameObject);
    }

    public void OnWaitTimeout()
    {

    }


    public void SetWaitGauge(float ratio)
    {
    }

    public UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (_actionExecutor.CurrentAction is WaitForOrderAction)
            OnTakeOrder();
        else if (_actionExecutor.CurrentAction is WaitForFoodAction)
            OnServed(character);

        return UniTask.CompletedTask;
    }

    private void OnTakeOrder()
    {
        _actionExecutor.CancelCurrentAction();
        Order = _orderService.AddOrder();
        EnqueueAction(new WaitForFoodAction(this));
    }

    private void OnServed(CharacterBase character)
    {
        // 음식이 서빙됐을 때의 로직 (예: 음식 확인, 먹기 시작)
        Debug.Log($"[CustomerCharacter] Served with {character.CurrentCarriable.GetCarriableType()}");
    }

    public void SetHighlight(bool highlight)
    {
        throw new System.NotImplementedException();
    }
}
