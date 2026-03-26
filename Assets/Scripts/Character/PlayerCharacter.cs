using UnityEngine;
using VContainer;
using MessagePipe;
using System;

public class PlayerCharacter : CharacterBase
{
    [SerializeField] NavMeshMover _mover;
    [SerializeField] ActionExecutor _actionExecutor;

    ISubscriber<MoveEvent> _moveSubscriber;
    ISubscriber<InteractEvent> _interactSubscriber;
    IDisposable _moveDisposable;
    IDisposable _interactDisposable;

    [Inject]
    public void Construct(
        ISubscriber<MoveEvent> moveSubscriber,
        ISubscriber<InteractEvent> interactSubscriber)
    {
        _moveSubscriber = moveSubscriber;
        _interactSubscriber = interactSubscriber;
    }

    void OnEnable()
    {
        var bag = DisposableBag.CreateBuilder();
        _moveSubscriber.Subscribe(OnMoveEvent).AddTo(bag);
        _interactSubscriber.Subscribe(OnInteractEvent).AddTo(bag);
        bag.Build();
    }

    void OnDisable()
    {
        _moveDisposable?.Dispose();
        _interactDisposable?.Dispose();
    }

    void OnMoveEvent(MoveEvent e)
    {
        _actionExecutor.Clear();
        _actionExecutor.Enqueue(new MoveAction(_mover, e.Destination));
    }

    void OnInteractEvent(InteractEvent e)
    {
        _actionExecutor.Clear();
        _actionExecutor.Enqueue(new InteractAction(_mover, e.Target, this));
    }
}
