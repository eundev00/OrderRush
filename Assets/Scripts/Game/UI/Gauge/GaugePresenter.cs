using System;
using UnityEngine;
using VContainer;

public class GaugePresenter : IDisposable
{
    public GaugeView View { get; private set; }

    private readonly Transform _target;
    private readonly Vector3 _offset;

    private RectTransform _canvasRectTransform;
    private bool _isDisposed;
    private Camera _mainCamera;

    public GaugePresenter(Camera mainCamera, RectTransform canvasRectTransform, GaugeView view, Transform target, Vector3 offset, Sprite icon = null)
    {
        View = view;
        _target = target;
        _offset = offset;
        _mainCamera = mainCamera;
        _canvasRectTransform = canvasRectTransform;

        if (icon != null)
        {
            View.SetIcon(icon);
        }
    }


    public void UpdatePosition()
    {
        if (_isDisposed || View == null || _target == null)
        {
            return;
        }

        Vector3 worldPosition = _target.position + _offset;
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(worldPosition);

        // 화면 밖이면 숨김
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

        // 화면 안이면 표시
        if (!View.gameObject.activeSelf)
        {
            View.Show();
        }

        // 스크린 좌표 → UI 좌표 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            screenPosition,
            null,
            out Vector2 localPoint
        );

        View.transform.localPosition = localPoint;
    }

    public void SetProgress(float progress)
    {
        if (_isDisposed || View == null) return;
        View.SetProgress(progress);
    }

    public void SetColor(Color color)
    {
        if (_isDisposed || View == null) return;
        View.SetColor(color);
    }

    public void SetIcon(Sprite sprite)
    {
        if (_isDisposed || View == null) return;
        View.SetIcon(sprite);
    }

    public void Show()
    {
        if (_isDisposed || View == null) return;
        View.Show();
    }

    public void Hide()
    {
        if (_isDisposed || View == null) return;
        View.Hide();
    }

    public bool IsTargetDestroyed()
    {
        return _target == null;
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        View = null;
    }

}
