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

        // InteractPoint로 이동 가능한지 확인
        if (!NavMesh.SamplePosition(interactPoint, out var navHit, 2.0f, NavMesh.AllAreas))
        {
            //     Debug.LogWarning($"[InteractAction] Cannot navigate to {_target.GetType().Name} at {interactPoint}");
            return;
        }

        // InteractPoint로 이동
        _animator.SetSpeed(1f);
        await _mover.MoveToAsync(navHit.position, ct);
        _animator.SetSpeed(0f);


        // 타겟을 바라보도록 회전
        Vector3 lookDirection;
        if (_target.InteractPoint.parent != null)
        {
            lookDirection = _target.InteractPoint.parent.position - _character.transform.position;
        }
        else
        {
            lookDirection = _target.InteractPoint.position - _character.transform.position;
        }

        lookDirection.y = 0;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            _character.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // 상호작용 실행
        _animator.SetWorking(true);
        await _target.InteractAsync(_character, ct);
        _animator.SetWorking(false);
    }
}
