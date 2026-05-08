using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LeaveAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    private readonly Vector3 _exitPosition;
    private readonly NavMeshMover _mover;
    private readonly CharacterAnimator _animator;

    public LeaveAction(
        CustomerCharacter customer,
        Vector3 exitPosition,
        NavMeshMover mover,
        CharacterAnimator animator)
    {
        _customer = customer;
        _exitPosition = exitPosition;
        _mover = mover;
        _animator = animator;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        _customer.EnableNavMeshAgent();
        await new MoveAction(_mover, _exitPosition, _animator).ExecuteAsync(ct);
        Object.Destroy(_customer.gameObject);
    }
}
