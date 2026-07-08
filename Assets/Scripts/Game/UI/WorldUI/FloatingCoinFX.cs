using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FloatingCoinFX : MonoBehaviour, IUIView
{
    [NotNull][SerializeField] private RectTransform _coinRect;

    [Header("Rise")]
    [SerializeField] private float riseHeight = 100f;
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

    public async UniTask PlayAnimation(CancellationToken ct = default)
    {
        _sequence?.Kill();

        _coinRect.localPosition = Vector3.zero;

        _sequence = DOTween.Sequence();

        // 상승
        _sequence.Append(_coinRect.DOLocalMoveY(riseHeight, riseDuration).SetEase(riseEase));

        _sequence.AppendInterval(holdDuration);

        // 하강
        _sequence.Append(_coinRect.DOLocalMoveY(0f, fallDuration).SetEase(fallEase));

        ct.Register(() => _sequence?.Kill());
        await _sequence.AsyncWaitForCompletion();
    }

    private void OnDisable()
    {
        _sequence?.Kill();
    }
}