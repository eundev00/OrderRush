using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using Services.UpdateService;
using VContainer;
using UnityEngine.EventSystems;

public class PlayerInputHandler : MonoBehaviour, IUpdatable
{
    [SerializeField] NavMeshMover _mover;
    [SerializeField] LayerMask _groundLayer;

    CancellationTokenSource _cts = new();
    Camera _mainCamera;
    IUpdateSubscriptionService _updateService;

    [Inject]
    public void Construct(IUpdateSubscriptionService updateService)
    {
        _updateService = updateService;
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    void OnEnable()
    {
        _updateService?.RegisterUpdatable(this);
    }

    void OnDisable()
    {
        _updateService?.UnregisterUpdatable(this);
    }

    public void ManagedUpdate()
    {
#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (!mouse.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current.IsPointerOverGameObject()) return; // UI 클릭 방지

        var ray = _mainCamera.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 100f, _groundLayer))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out var navHit, 1f, UnityEngine.AI.NavMesh.AllAreas))
                MoveToPosition(navHit.position);
        }
    }

    void HandleTouchInput()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null) return;

        var touch = touchscreen.primaryTouch;
        if (!touch.press.wasPressedThisFrame) return;

        var ray = _mainCamera.ScreenPointToRay(touch.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 100f, _groundLayer))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out var navHit, 1f, UnityEngine.AI.NavMesh.AllAreas))
                MoveToPosition(navHit.position);
        }
    }

    void MoveToPosition(Vector3 position)
    {
        CancelMove();
        _mover.MoveToAsync(position, _cts.Token).Forget();
    }

    void CancelMove()
    {
        if (_cts is null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
    }

    void OnDestroy()
    {
        if (_cts is null) return;
        _cts.Cancel();
        _cts.Dispose();
    }
}