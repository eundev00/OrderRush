using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class FloatingCoinFX : MonoBehaviour, IUIView
{
    [Header("Rise")]
    [SerializeField] private float riseHeight = 1f;
    [SerializeField] private float riseDuration = 0.35f;
    [SerializeField] private Ease riseEase = Ease.OutBack;

    [Header("Hold")]
    [SerializeField] private float holdDuration = 0.2f;

    [Header("Fall")]
    [SerializeField] private float fallDuration = 0.3f;
    [SerializeField] private Ease fallEase = Ease.InBack;

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationAnglePerPhase = 360f;

    private Sequence _sequence;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public async UniTask PlayAnimation(Vector3 worldPosition, CancellationToken ct = default)
    {
        _sequence?.Kill();

        transform.position = worldPosition;
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _sequence = DOTween.Sequence();

        // 상승: 팝업 스케일 + 위로 이동 + 회전 1회 (동시 진행)
        _sequence.Append(transform.DOScale(1f, riseDuration).SetEase(riseEase));
        _sequence.Join(transform.DOMoveY(worldPosition.y + riseHeight, riseDuration).SetEase(riseEase));
        _sequence.Join(transform
            .DOLocalRotate(rotationAxis * rotationAnglePerPhase, riseDuration, RotateMode.FastBeyond360)
            .SetRelative()
            .SetEase(Ease.Linear));

        _sequence.AppendInterval(holdDuration);

        // 하강: 아래로 이동 + 회전 1회 + 스케일 축소 (동시 진행)
        _sequence.Append(transform.DOMoveY(worldPosition.y, fallDuration).SetEase(fallEase));
        _sequence.Join(transform
            .DOLocalRotate(rotationAxis * rotationAnglePerPhase, fallDuration, RotateMode.FastBeyond360)
            .SetRelative()
            .SetEase(Ease.Linear));
        _sequence.Join(transform.DOScale(0f, fallDuration).SetEase(fallEase));

        ct.Register(() => _sequence?.Kill());
        await _sequence.AsyncWaitForCompletion();
    }

    private void OnDisable()
    {
        _sequence?.Kill();
    }
}