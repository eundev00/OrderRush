using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaitInLineAction : IGameAction
{
    private readonly CharacterAnimator _animator;
    private readonly Transform _characterTransform;
    private readonly Quaternion _targetRotation;
    private readonly NavMeshMover _mover;

    public WaitInLineAction(CharacterAnimator animator, Transform characterTransform, Quaternion targetRotation, NavMeshMover mover)
    {
        _animator = animator;
        _characterTransform = characterTransform;
        _targetRotation = targetRotation;
        _mover = mover;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        _animator.Play(CharacterAnimState.Idle);
        _mover.DisableAgent();

        Quaternion startRotation = _characterTransform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < Constants.kRotationSeconds)
        {
            ct.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / Constants.kRotationSeconds);
            _characterTransform.rotation = Quaternion.Slerp(startRotation, _targetRotation, t);
            await UniTask.Yield();
        }

        _characterTransform.rotation = _targetRotation;
        await UniTask.WaitUntilCanceled(ct);
    }
}
