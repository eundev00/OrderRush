using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class InteractAction : IGameAction
{
    private readonly NavMeshMover _mover;
    private readonly IInteractable _target;
    private readonly CharacterBase _character;

    public InteractAction(NavMeshMover mover, IInteractable target, CharacterBase character)
    {
        _mover = mover;
        _target = target;
        _character = character;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        var interactPoint = _target.InteractPoint.position;

        // InteractPoint로 이동 가능한지 확인
        if (!NavMesh.SamplePosition(interactPoint, out var navHit, 0.5f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"Cannot navigate to {_target.DisplayName}");
            return;
        }

        // InteractPoint로 이동
        await _mover.MoveToAsync(navHit.position, ct);

        // 상호작용 실행
        await _target.InteractAsync(_character, ct);
    }
}
