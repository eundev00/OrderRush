using A1.Meta;
using UnityEngine;

public enum CharacterAnimState
{
    Idle,
    IdleHold,
    Walking,
    WalkingHold,
    Working,
    PickUp,
    PutDown
}

public class CharacterAnimator : MonoBehaviour
{
    static readonly int IdleHash = Animator.StringToHash("Idle");
    static readonly int IdleHoldHash = Animator.StringToHash("IdleHold");
    static readonly int WalkingHash = Animator.StringToHash("Walking");
    static readonly int WalkingHoldHash = Animator.StringToHash("WalkingHold");
    static readonly int WorkingHash = Animator.StringToHash("Working");
    static readonly int PickUpHash = Animator.StringToHash("PickUp");
    static readonly int PutDownHash = Animator.StringToHash("PutDown");

    const string PickUpClipName = "CharacterAnim_PutDown";
    const string PutDownClipName = "CharacterAnim_PutDown";
    const float PickUpStateSpeed = 2f;
    const float PutDownStateSpeed = 2f;
    const float DefaultTransitionDuration = 0.1f;

    [NotNull][SerializeField] Animator _animator;

    CharacterAnimState _currentState;
    bool _isHolding;
    float _pickUpDuration;
    float _putDownDuration;

    public void Play(CharacterAnimState state)
    {
        if (_animator == null) return;

        int hash;
        float transitionDuration = DefaultTransitionDuration;

        switch (state)
        {
            case CharacterAnimState.Idle:
                hash = _isHolding ? IdleHoldHash : IdleHash;
                state = _isHolding ? CharacterAnimState.IdleHold : CharacterAnimState.Idle;
                break;
            case CharacterAnimState.Walking:
                hash = _isHolding ? WalkingHoldHash : WalkingHash;
                state = _isHolding ? CharacterAnimState.WalkingHold : CharacterAnimState.Walking;
                break;
            case CharacterAnimState.PickUp:
                hash = PickUpHash;
                transitionDuration = 0f;
                _isHolding = true;
                break;
            case CharacterAnimState.PutDown:
                hash = PutDownHash;
                transitionDuration = 0f;
                _isHolding = false;
                break;
            case CharacterAnimState.Working:
                hash = WorkingHash;
                break;
            default:
                return;
        }

        if (_currentState == state) return;
        _currentState = state;
        _animator.CrossFade(hash, transitionDuration, 0);
    }

    public float GetPickUpDuration()
    {
        if (_pickUpDuration <= 0f)
            _pickUpDuration = GetPlaybackDuration(PickUpClipName, PickUpStateSpeed);
        return _pickUpDuration;
    }

    public float GetPutDownDuration()
    {
        if (_putDownDuration <= 0f)
            _putDownDuration = GetPlaybackDuration(PutDownClipName, PutDownStateSpeed);
        return _putDownDuration;
    }

    float GetPlaybackDuration(string clipName, float stateSpeed)
    {
        if (_animator == null || _animator.runtimeAnimatorController == null) return 0f;
        return _animator.GetAnimationLength(clipName) / stateSpeed;
    }
}
