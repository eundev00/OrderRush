using UnityEngine;

public class WorldUITracker : MonoBehaviour
{
    private Transform _target;
    private Vector3 _offset;
    private Camera _camera;
    private RectTransform _canvasRect;
    private RectTransform _rectTransform;
    private bool _isInitialized;

    public void Initialize(Camera camera, RectTransform canvas, Transform target, Vector3 offset)
    {
        _camera = camera;
        _canvasRect = canvas;
        _target = target;
        _offset = offset;
        _rectTransform = GetComponent<RectTransform>();
        _isInitialized = true;

        UpdatePosition();
    }

    private void LateUpdate()
    {
        if (!_isInitialized || _target == null) return;

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 worldPos = _target.position + _offset;
        Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0 ||
            screenPos.x < 0 || screenPos.x > Screen.width ||
            screenPos.y < 0 || screenPos.y > Screen.height)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screenPos, null, out Vector2 localPoint);
        _rectTransform.localPosition = localPoint;
    }
}
