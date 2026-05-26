using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

public class LeaveAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    private readonly Vector3 _exitPosition;
    private readonly NavMeshMover _mover;
    private readonly CharacterAnimator _animator;
    private readonly IPublisher<CustomerRemovedEvent> _customerRemovedPublisher;

    public LeaveAction(
        CustomerCharacter customer,
        Vector3 exitPosition,
        NavMeshMover mover,
        CharacterAnimator animator,
        IPublisher<CustomerRemovedEvent> customerRemovedPublisher)
    {
        _customer = customer;
        _exitPosition = exitPosition;
        _mover = mover;
        _animator = animator;
        _customerRemovedPublisher = customerRemovedPublisher;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_customer == null || _customer.gameObject == null)
        {
            return;
        }

        if (_customer.AssignedTable != null)
        {
            _customer.AssignedTable.CustomerLeaving(_customer.AssignedSeatIndex);
        }

        try
        {
            _customer.EnableNavMeshAgent();
            await new MoveAction(_mover, _exitPosition, _animator).ExecuteAsync(ct);
        }
        finally
        {
            if (_customer != null && _customer.gameObject != null)
            {
                _customerRemovedPublisher.Publish(new CustomerRemovedEvent(_customer.IsServed));
                Object.Destroy(_customer.gameObject);
            }
        }
    }
}
