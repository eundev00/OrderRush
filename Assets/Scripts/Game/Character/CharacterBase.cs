using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MessagePipe;
using VContainer;

public abstract class CharacterBase : MonoBehaviour
{
    private IDisposable _dayEndedSubscription;
    [NotNull][SerializeField] protected Transform _itemSlot;
    [NotNull][SerializeField] protected ActionExecutor _actionExecutor;
    [NotNull][SerializeField] protected NavMeshMover _mover;
    [NotNull][SerializeField] protected CharacterAnimator _animator;

    public bool IsHolding => CurrentCarriable != null;
    public ICarriable CurrentCarriable { get; protected set; }

    public Transform ItemSlot => _itemSlot;
    public bool IsExecuting => _actionExecutor.IsExecuting();

    [Inject]
    public void Construct(ISubscriber<DayEndedEvent> dayEndedSubscriber)
    {
        _dayEndedSubscription = dayEndedSubscriber.Subscribe(_ => OnDayEnded());
    }

    protected virtual void OnDayEnded()
    {
        _actionExecutor.Clear();
    }

    void OnDestroy()
    {
        _dayEndedSubscription?.Dispose();
    }

    public async UniTask PickUp(ICarriable item)
    {
        if (item is null) return;

        try
        {
            float length = _animator.GetPickUpLength();
            _animator.TriggerPickUp();
            CurrentCarriable = item;
            CurrentCarriable.AttachToSlot(ItemSlot);

            await UniTask.Delay(TimeSpan.FromSeconds(length));
        }
        catch (OperationCanceledException ex)
        {
            Debug.Log($"PickUp canceled: {ex}");
        }
        catch (System.Exception ex)
        {
            Debug.Log($" Error during PickUp: {ex}");
        }
    }


    public async UniTask PutDown()
    {
        if (CurrentCarriable == null) return;

        try
        {
            float length = _animator.GetPickUpLength();
            _animator.TriggerPutDown();
            CurrentCarriable = null;
            await UniTask.Delay(TimeSpan.FromSeconds(length));
        }
        catch (OperationCanceledException ex)
        {
            Debug.Log($"PutDown canceled: {ex}");
        }
        catch (System.Exception ex)
        {
            Debug.Log($" Error during PutDown: {ex}");
        }

        return;
    }


    public async UniTask<ICarriable> PutDownAt(Transform attachSlot)
    {
        if (CurrentCarriable == null) return null;

        try
        {
            var carriedItem = CurrentCarriable;
            float length = _animator.GetPickUpLength();
            _animator.TriggerPutDown();
            CurrentCarriable.AttachToSlot(attachSlot);
            CurrentCarriable = null;
            await UniTask.Delay(TimeSpan.FromSeconds(length));
            return carriedItem;
        }
        catch (OperationCanceledException ex)
        {
            Debug.Log($"PutDown canceled: {ex}");
        }
        catch (System.Exception ex)
        {
            Debug.Log($" Error during PutDown: {ex}");
        }

        return null;
    }

    public void StartWorking()
    {
        _animator.SetWorking(true);
    }

    public void StopWorking()
    {
        _animator.SetWorking(false);
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
