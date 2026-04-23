using UnityEngine;
using VContainer;
using MessagePipe;
using System;

public class PlayerCharacter : CharacterBase
{
    IDisposable _subscription;

    [Inject]
    public void Construct(
        ISubscriber<MoveEvent> moveSubscriber,
        ISubscriber<InteractEvent> interactSubscriber)
    {
        var bag = DisposableBag.CreateBuilder();
        moveSubscriber.Subscribe(OnMoveEvent).AddTo(bag);
        interactSubscriber.Subscribe(OnInteractEvent).AddTo(bag);
        _subscription = bag.Build();
    }

    void OnDestroy()
    {
        _subscription?.Dispose();
    }

    void OnMoveEvent(MoveEvent e)
    {
        _actionExecutor.Clear();
        _actionExecutor.Enqueue(new MoveAction(_mover, e.Destination, _animator));
    }

    void OnInteractEvent(InteractEvent e)
    {
        _actionExecutor.Clear();
        _actionExecutor.Enqueue(new InteractAction(_mover, e.Target, this, _animator));
    }
}
