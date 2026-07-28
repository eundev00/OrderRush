using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CameraDirector : MonoBehaviour, ICameraDirector
{
    [Header("Target")]
    [SerializeField] Camera _camera;

    [Header("Zoom")]
    [SerializeField][Range(0f, 12f)] float _zoomOutDistance = 7f;
    [SerializeField] float _zoomOutFovDelta = 0f;

    [Header("Zoom In")]
    [SerializeField] float _zoomInDuration = 0.8f;
    [SerializeField] Ease _zoomInEase = Ease.OutQuint;

    [Header("Zoom Out")]
    [SerializeField] float _zoomOutDuration = 0.5f;
    [SerializeField] Ease _zoomOutEase = Ease.InOutCubic;

    Transform _cameraTransform;
    Vector3 _basePosition;
    float _baseFov;
    Sequence _sequence;

    Vector3 ZoomOutPosition => _basePosition - _cameraTransform.forward * _zoomOutDistance;

    void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("[CameraDirector] Camera 를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        _cameraTransform = _camera.transform;
        _basePosition = _cameraTransform.position;
        _baseFov = _camera.fieldOfView;

        // 최초 진입은 줌아웃 상태에서 시작한다
        _cameraTransform.position = ZoomOutPosition;
        _camera.fieldOfView = _baseFov + _zoomOutFovDelta;
    }

    void OnDestroy()
    {
        _sequence?.Kill();
    }

    public UniTask PlayDayIntroAsync()
        => PlayAsync(_basePosition, _baseFov, _zoomInDuration, _zoomInEase);

    public UniTask PlayDayOutroAsync()
        => PlayAsync(ZoomOutPosition, _baseFov + _zoomOutFovDelta, _zoomOutDuration, _zoomOutEase);

    UniTask PlayAsync(Vector3 targetPosition, float targetFov, float duration, Ease ease)
    {
        _sequence?.Kill();

        // tcs 는 반드시 로컬 캡처. 필드로 두면 다음 호출의 Kill 이 이전 OnKill 을 발화시켜 새 await 가 즉시 풀린다.
        var tcs = new UniTaskCompletionSource();

        _sequence = DOTween.Sequence()
            .Join(_cameraTransform.DOMove(targetPosition, duration).SetEase(ease));

        if (!Mathf.Approximately(_camera.fieldOfView, targetFov))
        {
            float fromFov = _camera.fieldOfView;
            _sequence.Join(DOVirtual
                .Float(fromFov, targetFov, duration, v => _camera.fieldOfView = v)
                .SetEase(ease));
        }

        _sequence
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            .SetUpdate(true)
            .OnComplete(() => tcs.TrySetResult())
            .OnKill(() => tcs.TrySetCanceled());

        return tcs.Task;
    }
}
