using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using VContainer;

public class FloatingCoinFX : MonoBehaviour
{
    private ISoundService _soundService;

    [Header("Rise")]
    [SerializeField] private float riseHeight = 2f;
    [SerializeField] private float riseDuration = 0.35f;
    [SerializeField] private Ease riseEase = Ease.OutBack;

    [Header("Hold")]
    [SerializeField] private float holdDuration = 0.2f;

    [Header("Fall")]
    [SerializeField] private float fallDuration = 0.3f;
    [SerializeField] private Ease fallEase = Ease.InBack;

    private Sequence _sequence;
    private Vector3 _startPosition;

    [Inject]
    public void Construct(ISoundService soundService)
    {
        _soundService = soundService;
    }

    public async UniTask PlayAnimation(CancellationToken ct = default)
    {
        _sequence?.Kill();

        _soundService?.PlaySfx(AudioKeys.coin_drop1);

        _startPosition = transform.position;

        await UniTask.NextFrame(cancellationToken: ct);

        _sequence = DOTween.Sequence();

        _sequence.Append(transform.DOMoveY(_startPosition.y + riseHeight, riseDuration).SetEase(riseEase));
        _sequence.AppendInterval(holdDuration);
        _sequence.Append(transform.DOMoveY(_startPosition.y, fallDuration).SetEase(fallEase));

        ct.Register(() => _sequence?.Kill());
        await _sequence.AsyncWaitForCompletion();
    }

    private void OnDisable()
    {
        _sequence?.Kill();
    }
}