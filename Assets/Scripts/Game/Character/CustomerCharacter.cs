using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using OrderRush.Services;
using UnityEngine;
using VContainer;

public class CustomerCharacter : CharacterBase
{
    public int OrderedRecipeID { get; set; } = -1;
    public float OrderedTime { get; set; }
    public DiningTable AssignedTable { get; private set; }
    public int AssignedSeatIndex { get; private set; }
    public bool IsServed { get; set; }

    private IAccountService _accountService;
    private IGameDataService _gameDataService;
    private Vector3 _spawnPosition;

    private IPublisher<PaymentEvent> _paymentPublisher;
    private IPublisher<CustomerRemovedEvent> _customerRemovedPublisher;
    private WorldUIFactory _worldUIFactory;

    [Inject]
    public void Construct(
        IPublisher<PaymentEvent> paymentPublisher,
        IPublisher<CustomerRemovedEvent> customerRemovedPublisher,
        IAccountService accountService,
        IGameDataService gameDataService,
        WorldUIFactory worldUIFactory)
    {
        _paymentPublisher = paymentPublisher;
        _customerRemovedPublisher = customerRemovedPublisher;
        _accountService = accountService;
        _gameDataService = gameDataService;
        _worldUIFactory = worldUIFactory;
    }

    protected override void OnDayEnded()
    {
        EnqueueLeave();
    }

    public void StopAllActions()
    {
        ClearActions();
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
        EnqueueAction(new LeaveAction(this, _spawnPosition, _mover, _animator, _customerRemovedPublisher));
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

        IsServed = true;

        EnqueueAction(new EatAction(this, _paymentPublisher, _gameDataService, _worldUIFactory));
        EnqueueAction(new LeaveAction(this, _spawnPosition, _mover, _animator, _customerRemovedPublisher));
    }


    public void EnqueueTakeOrder()
    {
        if (OrderedRecipeID != -1)
            return;

        if (_actionExecutor.CurrentAction is WaitForOrderAction)
        {
            _actionExecutor.CancelCurrentAction();
        }

        EnqueueAction(new OrderAction(this, _accountService));
        EnqueueAction(new WaitForFoodAction(this, _worldUIFactory, _gameDataService));
    }

    public void EnqueueLeaveAngry()
    {
        ClearActions();
        EnqueueAction(new EmoteAction(this, _worldUIFactory));
        EnqueueAction(new LeaveAction(this, _spawnPosition, _mover, _animator, _customerRemovedPublisher));
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
