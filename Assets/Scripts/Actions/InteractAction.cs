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

        // 타겟을 바라보도록 회전
        Vector3 lookDirection;
        if (_target.InteractPoint.parent != null)
        {
            // InteractPoint의 부모(실제 인터랙션 오브젝트)를 바라봄
            lookDirection = _target.InteractPoint.parent.position - _character.transform.position;
        }
        else
        {
            // 부모가 없으면 InteractPoint를 바라봄
            lookDirection = _target.InteractPoint.position - _character.transform.position;
        }

        lookDirection.y = 0; // 수평으로만 회전
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            _character.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // 상호작용 실행
        await _target.InteractAsync(_character, ct);
    }
}
