using A1.Meta;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int IsWorkingHash = Animator.StringToHash("IsWorking");

    static readonly int PickUpHash = Animator.StringToHash("PickUp");
    static readonly int PutDownHash = Animator.StringToHash("PutDown");

    [NotNull][SerializeField] Animator _animator;

    public void SetSpeed(float speed)
    {
        if (_animator == null) return;
        _animator.SetFloat(SpeedHash, speed);
    }

    public void SetWorking(bool isWorking)
    {
        if (_animator == null) return;
        _animator.SetBool(IsWorkingHash, isWorking);
    }

    public void TriggerPickUp()
    {
        if (_animator == null) return;
        _animator.SetTrigger(PickUpHash);
    }

    public void TriggerPutDown()
    {
        if (_animator == null) return;
        _animator.SetTrigger(PutDownHash);
    }

    public float GetPickUpLength()
    {
        if (_animator == null) return 0f;
        return _animator.GetAnimationLength("CharacterAnim_PutDown");
    }

}