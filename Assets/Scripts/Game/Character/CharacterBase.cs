using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MessagePipe;
using VContainer;

public abstract class CharacterBase : MonoBehaviour
{
    private IDisposable _dayEndedSubscription;
    private IDisposable _gameCleanupSubscription;
    [NotNull][SerializeField] protected Transform _itemSlot;
    [NotNull][SerializeField] protected ActionExecutor _actionExecutor;
    [NotNull][SerializeField] protected NavMeshMover _mover;
    [NotNull][SerializeField] protected CharacterAnimator _animator;

    public bool IsHolding => CurrentCarriable != null;
    protected ICarriable _currentCarriable;
    public ICarriable CurrentCarriable
    {
        get => _currentCarriable;
        protected set => _currentCarriable = value;
    }

    public Transform ItemSlot => _itemSlot;
    public bool IsExecuting => _actionExecutor.IsExecuting();

    protected virtual void Awake()
    {
        _actionExecutor.StartLoop();
    }

    [Inject]
    public void Construct(ISubscriber<DayEndedEvent> dayEndedSubscriber, ISubscriber<GameCleanupEvent> gameCleanupSubscriber)
    {
        _dayEndedSubscription = dayEndedSubscriber.Subscribe(_ => OnDayEnded());
        _gameCleanupSubscription = gameCleanupSubscriber.Subscribe(_ => OnGameCleanup());
    }

    protected virtual void OnDayEnded()
    {
        _actionExecutor.Clear();
    }

    protected virtual void OnGameCleanup()
    {
        _currentCarriable?.DestroyObject();
        _currentCarriable = null;
    }

    protected virtual void OnDestroy()
    {
        _actionExecutor.StopLoop();
        _dayEndedSubscription?.Dispose();
        _gameCleanupSubscription?.Dispose();
    }

    public async UniTask PickUp(ICarriable item, CancellationToken ct)
    {
        if (item is null) return;

        _animator.Play(CharacterAnimState.PickUp);
        CurrentCarriable = item;
        CurrentCarriable.AttachToSlot(ItemSlot);

        await DelayAnimation(_animator.GetPickUpDuration(), ct);
        _animator.Play(CharacterAnimState.Idle);
    }


    public async UniTask PutDown(CancellationToken ct)
    {
        if (CurrentCarriable == null) return;

        CurrentCarriable = null;
        _animator.Play(CharacterAnimState.PutDown);
        await DelayAnimation(_animator.GetPutDownDuration(), ct);
        _animator.Play(CharacterAnimState.Idle);
    }


    public async UniTask<ICarriable> PutDownAt(Transform attachSlot, CancellationToken ct)
    {
        if (CurrentCarriable == null) return null;

        var carriedItem = CurrentCarriable;
        _animator.Play(CharacterAnimState.PutDown);
        await DelayAnimation(_animator.GetPutDownDuration(), ct);

        CurrentCarriable.AttachToSlot(attachSlot);
        CurrentCarriable = null;
        _animator.Play(CharacterAnimState.Idle);

        return carriedItem;
    }

    private static async UniTask DelayAnimation(float duration, CancellationToken ct)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void StartWorking()
    {
        _animator.Play(CharacterAnimState.Working);
    }

    public void StopWorking()
    {
        _animator.Play(CharacterAnimState.Idle);
    }

    protected void EnqueueAction(IGameAction action)
    {
        _actionExecutor.Enqueue(action);
    }

    protected void ClearActions()
    {
        _actionExecutor.Clear();
    }

    public void EnableNavMeshAgent()
    {
        if (_mover != null)
        {
            _mover.EnableAgent();
        }
    }

    public void DisableNavMeshAgent()
    {
        if (_mover != null)
        {
            _mover.DisableAgent();
        }
    }

    public void WarpTo(Vector3 position)
    {
        if (_mover != null)
        {
            _mover.Warp(position);
        }
    }
}
