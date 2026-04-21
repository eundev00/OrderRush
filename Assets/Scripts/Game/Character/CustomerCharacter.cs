using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using VContainer;

public class CustomerCharacter : CharacterBase
{
    public Order Order { get; private set; }
    public DiningSeat AssignedSeat { get; private set; }

    private const float DEFAULT_TIME_LIMIT = 60f;
    private const float EAT_DURATION = 5f;

    private Vector3 _spawnPosition;
    private IPublisher<PaymentEvent> _paymentPublisher;

    [Inject]
    public void Construct(IPublisher<PaymentEvent> paymentPublisher)
    {
        _paymentPublisher = paymentPublisher;
    }

    public void SetSpawnPosition(Vector3 position)
    {
        _spawnPosition = position;
    }

    public void GoToSeat(DiningSeat targetSeat, RecipeData recipe)
    {
        AssignedSeat = targetSeat;

        // 이동 → 착석 → 주문 생성
        EnqueueAction(new MoveAction(_mover, targetSeat.SitPoint.position));
        EnqueueAction(new SitAction(this, targetSeat));
        EnqueueAction(new OrderAction(this, recipe));
    }

    public void CreateOrder(RecipeData recipe)
    {
        Order = new Order(recipe, DEFAULT_TIME_LIMIT);
        Debug.Log($"[CustomerCharacter] Order created: {recipe.RecipeName} (Time limit: {DEFAULT_TIME_LIMIT}s)");
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
        EnqueueAction(new MoveAction(_mover, _spawnPosition));

        // 이동 완료 후 사라지기 (ActionExecutor의 큐가 비워지면)
        await UniTask.WaitUntil(() => !IsExecuting);

        Debug.Log($"[CustomerCharacter] Destroying...");
        Destroy(gameObject);
    }
}
