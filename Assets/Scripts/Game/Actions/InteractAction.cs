using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class InteractAction : IGameAction
{
    private readonly NavMeshMover _mover;
    private readonly IInteractable _target;
    private readonly CharacterBase _character;
    private readonly CharacterAnimator _animator;

    public InteractAction(NavMeshMover mover, IInteractable target, CharacterBase character, CharacterAnimator animator)
    {
        _mover = mover;
        _target = target;
        _character = character;
        _animator = animator;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        var interactPoint = _target.InteractPoint.position;

        if (!NavMesh.SamplePosition(interactPoint, out var navHit, 2.0f, NavMesh.AllAreas))
        {
            return;
        }

        try
        {
            _target.SetHighlight(true);
            _animator.SetSpeed(1f);
            await _mover.MoveToAsync(navHit.position, ct);
            _animator.SetSpeed(0f);

            _character.transform.rotation = _target.InteractPoint.rotation;

            await _target.InteractAsync(_character, ct);
        }
        finally
        {
            _target.SetHighlight(false);
        }
    }
}
