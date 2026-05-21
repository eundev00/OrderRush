using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Services;
using UnityEngine;
using VContainer;

public class CustomerCharacter : CharacterBase
{
    public Order Order { get; set; }
    public DiningTable AssignedTable { get; private set; }
    public int AssignedSeatIndex { get; private set; }

    private IAccountService _accountService;
    private IGameDataService _gameDataService;
    private Vector3 _spawnPosition;

    private IPublisher<PaymentEvent> _paymentPublisher;
    private OrderIconFactory _orderIconFactory;
    private CharacterEmoteIconFactory _emoteIconFactory;

    [Inject]
    public void Construct(IPublisher<PaymentEvent> paymentPublisher,
        IAccountService accountService,
        IGameDataService gameDataService,
        OrderIconFactory orderIconFactory,
        CharacterEmoteIconFactory emoteIconFactory)
    {
        _paymentPublisher = paymentPublisher;
        _accountService = accountService;
        _gameDataService = gameDataService;
        _orderIconFactory = orderIconFactory;
        _emoteIconFactory = emoteIconFactory;
    }

    public void SetSpawnPosition(Vector3 position)
    {
        _spawnPosition = position;
    }

    public void EnqueueGoToSeat(DiningTable targetTable, int seatIndex)
    {
        AssignedTable = targetTable;
        AssignedSeatIndex = seatIndex;

        var targetSeat = targetTable.GetSeatTransform(seatIndex);
        if (targetSeat == null)
        {
            Debug.LogError($"[CustomerCharacter] Invalid seat index: {seatIndex}");
            return;
        }

        ClearActions();
        EnqueueAction(new MoveAction(_mover, targetSeat.position, _animator));
        EnqueueAction(new SitAction(this, AssignedTable, AssignedSeatIndex));
        EnqueueAction(new WaitForOrderAction());
    }

    public void EnqueueLeave()
    {
        ClearActions();
        EnqueueAction(new LeaveAction(this, _spawnPosition, _mover, _animator));
    }

    public void OnWaitTimeout()
    {
        EnqueueLeaveAngry();
    }

    public void EnqueueEatAndLeave()
    {
        if (_actionExecutor.CurrentAction is WaitForFoodAction)
        {
            _actionExecutor.CancelCurrentAction();
        }

        EnqueueAction(new EatAction(this, _paymentPublisher, _gameDataService));
        EnqueueAction(new LeaveAction(this, _spawnPosition, _mover, _animator));
    }


    public void EnqueueTakeOrder()
    {
        if (Order != null)
        {
            return;
        }

        if (_actionExecutor.CurrentAction is WaitForOrderAction)
        {
            _actionExecutor.CancelCurrentAction();
        }

        EnqueueAction(new OrderAction(this, _accountService, _gameDataService));
        EnqueueAction(new WaitForFoodAction(this, _orderIconFactory));
    }

    public void EnqueueLeaveAngry()
    {
        ClearActions();
        EnqueueAction(new EmoteAction(this, _emoteIconFactory));
        EnqueueAction(new LeaveAction(this, _spawnPosition, _mover, _animator));
    }

    public void EnqueueGoToWaitingPosition(Vector3 waitPosition, Quaternion waitRotation)
    {
        EnqueueAction(new MoveAction(_mover, waitPosition, _animator));
        EnqueueAction(new WaitInLineAction(_animator, transform, waitRotation, _mover));
    }

    public void EnqueueMoveToWaitingPosition(Vector3 waitPosition, Quaternion waitRotation)
    {
        ClearActions();
        EnqueueGoToWaitingPosition(waitPosition, waitRotation);
    }

}
