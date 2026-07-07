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
        var sortedPoints = _target.GetInteractPointsSortedByDistance(_character.transform.position);

        Transform validPoint = null;
        NavMeshHit navHit = default;

        foreach (var point in sortedPoints)
        {
            if (!NavMesh.SamplePosition(point.position, out navHit, 2.0f, NavMesh.AllAreas))
                continue;

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(_character.transform.position, navHit.position, NavMesh.AllAreas, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    validPoint = point;
                    break;
                }
            }
        }

        if (validPoint == null)
            return;

        try
        {
            _target.SetHighlight(true);
            _animator.SetSpeed(1f);
            await _mover.MoveToAsync(navHit.position, ct);
            _animator.SetSpeed(0f);

            _character.transform.rotation = validPoint.rotation;

            await _target.InteractAsync(_character, ct);
        }
        finally
        {
            _target.SetHighlight(false);
        }
    }
}
