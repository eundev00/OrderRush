using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using VContainer;

public class FloatingCoinFX : MonoBehaviour
{
    [NotNull][SerializeField] private RectTransform _coinRect;

    private ISoundService _soundService;

    [Inject]
    public void Construct(ISoundService soundService)
    {
        _soundService = soundService;
    }

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

    public async UniTask PlayAnimation(CancellationToken ct = default)
    {
        _sequence?.Kill();

        if (_coinRect == null)
        {
            Debug.LogError("[FloatingCoinFX] _coinRect is null!");
            return;
        }

        _soundService?.PlaySfx(AudioKeys.coin_drop1);

        _coinRect.localPosition = Vector3.zero;
        _coinRect.localRotation = Quaternion.identity;
        Debug.Log($"[FloatingCoinFX] Animation start - riseHeight: {riseHeight}, starting localPos: {_coinRect.localPosition}");

        _sequence = DOTween.Sequence();

        // 상승 + 회전
        _sequence.Append(_coinRect.DOLocalMoveY(riseHeight, riseDuration).SetEase(riseEase));
        _sequence.Join(_coinRect.DOLocalRotate(rotationAxis * rotationAnglePerPhase, riseDuration, RotateMode.LocalAxisAdd));

        _sequence.AppendInterval(holdDuration);

        // 하강 + 회전
        _sequence.Append(_coinRect.DOLocalMoveY(0f, fallDuration).SetEase(fallEase));
        _sequence.Join(_coinRect.DOLocalRotate(rotationAxis * rotationAnglePerPhase, fallDuration, RotateMode.LocalAxisAdd));

        _sequence.OnComplete(() => Debug.Log("[FloatingCoinFX] Animation complete"));

        ct.Register(() => _sequence?.Kill());
        await _sequence.AsyncWaitForCompletion();
    }

    private void OnDisable()
    {
        _sequence?.Kill();
    }
}