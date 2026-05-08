using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveAction : IGameAction
{
    private readonly NavMeshMover _mover;
    private readonly Vector3 _destination;
    private readonly CharacterAnimator _animator;

    public MoveAction(NavMeshMover mover, Vector3 destination, CharacterAnimator animator)
    {
        _mover = mover;
        _destination = destination;
        _animator = animator;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        _animator.SetSpeed(1f);

        try
        {
            await _mover.MoveToAsync(_destination, ct);
        }
        finally
        {
            _animator.SetSpeed(0f);
        }
    }
}
