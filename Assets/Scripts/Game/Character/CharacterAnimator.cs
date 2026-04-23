using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int IsWorkingHash = Animator.StringToHash("IsWorking");

    static readonly int PickUpHash = Animator.StringToHash("PickUp");
    static readonly int PutDownHash = Animator.StringToHash("PutDown");

    [SerializeField] Animator _animator;

    public void SetSpeed(float speed)
        => _animator.SetFloat(SpeedHash, speed);

    public void SetWorking(bool isWorking)
        => _animator.SetBool(IsWorkingHash, isWorking);

    public void TriggerPickUp() => _animator.SetTrigger(PickUpHash);
    public void TriggerPutDown() => _animator.SetTrigger(PutDownHash);
}