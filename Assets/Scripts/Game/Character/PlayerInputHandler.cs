using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;
using MessagePipe;
using UniRx;

public class PlayerInputHandler : ITickable, IDisposable
{
    private static readonly LayerMask RaycastMask = ~(1 << LayerMask.NameToLayer("Character"));
    readonly IPublisher<MoveEvent> _movePublisher;
    readonly IPublisher<InteractEvent> _interactPublisher;
    readonly ISubscriber<DayEndedEvent> _dayEndedSubscriber;
    readonly int _groundLayer;

    Camera _mainCamera;
    bool _isEnabled = true;
    readonly CompositeDisposable _disposables = new();

    [Inject]
    public PlayerInputHandler(
        IPublisher<MoveEvent> movePublisher,
        IPublisher<InteractEvent> interactPublisher,
        ISubscriber<DayEndedEvent> dayEndedSubscriber)
    {
        _movePublisher = movePublisher;
        _interactPublisher = interactPublisher;
        _dayEndedSubscriber = dayEndedSubscriber;
        _groundLayer = LayerMask.GetMask("Ground");

        _dayEndedSubscriber
            .Subscribe(_ => OnDayEnded())
            .AddTo(_disposables);
    }

    public void Tick()
    {
        if (!_isEnabled) return;

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_mainCamera == null)
            return;

#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void OnDayEnded()
    {
        _isEnabled = false;
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }

    void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (!mouse.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        var ray = _mainCamera.ScreenPointToRay(mouse.position.ReadValue());

        if (Physics.Raycast(ray, out var hit, 100f, RaycastMask))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                _interactPublisher.Publish(new InteractEvent(interactable));
                return;
            }
        }

        if (Physics.Raycast(ray, out var groundHit, 100f, _groundLayer))
        {
            if (NavMesh.SamplePosition(groundHit.point, out var navHit, 1f, NavMesh.AllAreas))
                _movePublisher.Publish(new MoveEvent(navHit.position));
        }
    }

    void HandleTouchInput()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null) return;

        var touch = touchscreen.primaryTouch;
        if (!touch.press.wasPressedThisFrame) return;

        var ray = _mainCamera.ScreenPointToRay(touch.position.ReadValue());

        if (Physics.Raycast(ray, out var hit, 100f, RaycastMask))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                _interactPublisher.Publish(new InteractEvent(interactable));
                return;
            }
        }

        if (Physics.Raycast(ray, out var groundHit, 100f, _groundLayer))
        {
            if (NavMesh.SamplePosition(groundHit.point, out var navHit, 1f, NavMesh.AllAreas))
                _movePublisher.Publish(new MoveEvent(navHit.position));
        }
    }
}
