using System;
using UnityEngine;

public abstract class BaseUIPresenter<TView> : IDisposable
    where TView : Component, IUIView
{
    public TView View { get; protected set; }

    protected readonly Transform _target;
    protected readonly Vector3 _offset;

    protected RectTransform _canvasRectTransform;
    protected bool _isDisposed;
    protected Camera _mainCamera;

    protected BaseUIPresenter(Camera mainCamera, RectTransform canvasRectTransform, TView view, Transform target, Vector3 offset)
    {
        View = view;
        _target = target;
        _offset = offset;
        _mainCamera = mainCamera;
        _canvasRectTransform = canvasRectTransform;
    }

    private void UpdatePosition()
    {
        if (_isDisposed || View == null || _target == null)
        {
            return;
        }

        Vector3 worldPosition = _target.position + _offset;
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z < 0 ||
            screenPosition.x < 0 || screenPosition.x > Screen.width ||
            screenPosition.y < 0 || screenPosition.y > Screen.height)
        {
            if (View.gameObject.activeSelf)
            {
                View.Hide();
            }
            return;
        }

        if (!View.gameObject.activeSelf)
        {
            View.Show();
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            screenPosition,
            null,
            out Vector2 localPoint
        );

        View.transform.localPosition = localPoint;
    }

    public virtual void Show()
    {
        if (_isDisposed || View == null) return;
        UpdatePosition();
    }

    public virtual void Hide()
    {
        if (_isDisposed || View == null) return;
        View.Hide();
    }

    public bool IsTargetDestroyed()
    {
        return _target == null;
    }

    public virtual void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        View = null;
    }
}
