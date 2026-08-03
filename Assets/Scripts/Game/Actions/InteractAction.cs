using System;
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
        var points = _target.GetInteractPointsSortedByDistance(_character.transform.position);

        Transform validPoint = null;
        NavMeshHit navHit = default;
        float shortestPathDistance = float.MaxValue;

        foreach (var point in points)
        {
            if (!NavMesh.SamplePosition(point.position, out var hit, 2.0f, NavMesh.AllAreas))
                continue;

            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(_character.transform.position, hit.position, NavMesh.AllAreas, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            float distance = CalculatePathDistance(path);
            if (distance < shortestPathDistance)
            {
                shortestPathDistance = distance;
                validPoint = point;
                navHit = hit;
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

            _target.SetHighlight(false);
            await _target.InteractAsync(_character, ct);
        }
        catch (OperationCanceledException)
        {
            if (_target is UnityEngine.Object obj && obj != null)
                _target.SetHighlight(false);
            if (_animator != null)
                _animator.SetSpeed(0f);
        }
    }

    private static float CalculatePathDistance(NavMeshPath path)
    {
        var corners = path.corners;
        float distance = 0f;
        for (int i = 1; i < corners.Length; i++)
            distance += Vector3.Distance(corners[i - 1], corners[i]);
        return distance;
    }
}
